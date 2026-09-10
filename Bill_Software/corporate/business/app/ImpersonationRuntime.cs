using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public enum ImpersonationStatus
    {
        Disabled = 0,
        Issued = 1,
        Started = 2,
        Closed = 3,
        ClosedNoOp = 4,
        Denied = 5,
        Fault = 6
    }

    public sealed class ImpersonationResult
    {
        public ImpersonationStatus Status { get; set; }
        public string Invariant { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public Guid? ImpersonationId { get; set; }

        public static ImpersonationResult Disabled()
        {
            return new ImpersonationResult { Status = ImpersonationStatus.Disabled, Message = "SwitchUser is disabled." };
        }

        public static ImpersonationResult Deny(string invariant, string message)
        {
            return new ImpersonationResult { Status = ImpersonationStatus.Denied, Invariant = invariant, Message = message };
        }

        public static ImpersonationResult Fail(string message)
        {
            return new ImpersonationResult { Status = ImpersonationStatus.Fault, Message = message };
        }
    }

    /// <summary>
    /// Administrator impersonation Start/Rollback engine. Every public entry
    /// returns Disabled with no SQL when SwitchUser is not exactly true.
    /// </summary>
    public static class ImpersonationRuntime
    {
        public static ImpersonationResult IssueIntent(int targetUserId)
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
                return ImpersonationResult.Disabled();

            ImpersonationResult gate = GateActor(targetUserId);
            if (gate != null)
                return gate;

            ActorContext actor;
            if (!TryReadActor(out actor))
                return ImpersonationResult.Fail("Actor session is incomplete.");

            var intent = new ImpersonationIntent
            {
                ImpersonationId = Guid.NewGuid(),
                ActorUserId = actor.UserId,
                TargetUserId = targetUserId,
                CompanyID = actor.CompanyID,
                ActorSessionToken = actor.SessionToken,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(ImpersonationGovernance.IntentTtlMinutes)
            };

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (SqlTransaction tran = cn.BeginTransaction())
                    {
                        SecurityAudit.Write(cn, tran, actor.UserId, SecurityAudit.ImpersonationIntentIssued,
                            FormatDetails(intent.ImpersonationId, actor.UserId, targetUserId, actor.CompanyID, "issued"));
                        tran.Commit();
                    }
                }
            }
            catch (SqlException)
            {
                return ImpersonationResult.Fail("Intent audit failed.");
            }

            return new ImpersonationResult
            {
                Status = ImpersonationStatus.Issued,
                Token = ImpersonationIntentToken.Issue(intent),
                ImpersonationId = intent.ImpersonationId
            };
        }

        public static ImpersonationResult Start(string intentToken)
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
                return ImpersonationResult.Disabled();

            ImpersonationIntent intent;
            if (!ImpersonationIntentToken.TryVerify(intentToken, out intent) || intent.ExpiresUtc <= DateTime.UtcNow)
                return DenyAndAudit(ImpersonationGovernance.InvIntent, "Intent token is invalid or expired.");

            ImpersonationResult gate = GateActor(intent.TargetUserId);
            if (gate != null)
                return gate;

            ActorContext actor;
            if (!TryReadActor(out actor))
                return ImpersonationResult.Fail("Actor session is incomplete.");

            if (intent.ActorUserId != actor.UserId
                || intent.CompanyID != actor.CompanyID
                || intent.ActorSessionToken != actor.SessionToken)
            {
                return DenyAndAudit(ImpersonationGovernance.InvIntent, "Intent token does not match the current actor.");
            }

            TargetProfile target;
            ImpersonationResult targetGate = LoadEligibleTarget(intent.TargetUserId, actor.CompanyID, out target);
            if (targetGate != null)
                return targetGate;

            Guid targetToken = Guid.NewGuid();
            DateTimeOffset leaseExpires = DateTimeOffset.UtcNow.AddMinutes(ImpersonationGovernance.LeaseDurationMinutes);
            string ip = ClientIp();
            string ua = ClientUserAgent();

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (SqlTransaction tran = cn.BeginTransaction())
                    {
                        try
                        {
                            InsertLease(cn, tran, intent, actor, targetToken, leaseExpires);
                        }
                        catch (SqlException ex)
                        {
                            if (IsUniqueViolation(ex))
                            {
                                tran.Rollback();
                                return DenyAndAudit(ImpersonationGovernance.InvIntent, "Intent token replay rejected.");
                            }
                            throw;
                        }

                        InsertActiveSession(cn, tran, targetToken, target.UserId, actor.CompanyID, ip, ua);
                        DeactivateSession(cn, tran, actor.SessionToken);

                        SecurityAudit.Write(cn, tran, actor.UserId, SecurityAudit.ImpersonationStart,
                            FormatDetails(intent.ImpersonationId, actor.UserId, target.UserId, actor.CompanyID, "started"));
                        tran.Commit();
                    }
                }
            }
            catch (SqlException)
            {
                return ImpersonationResult.Fail("Start failed.");
            }

            var link = new ImpersonationLink
            {
                ImpersonationId = intent.ImpersonationId,
                CompanyID = actor.CompanyID,
                ActorUserId = actor.UserId,
                ActorUserKey = actor.UserKey,
                ActorRoleId = actor.RoleId,
                ActorRoleName = actor.RoleName,
                ActorUserType = actor.UserType,
                ActorProfilePic = actor.ProfilePic,
                ActorSessionToken = actor.SessionToken,
                TargetUserId = target.UserId,
                TargetUserKey = target.UserKey,
                TargetRoleId = target.RoleId,
                TargetRoleName = target.RoleName,
                TargetProfilePic = target.ProfilePic,
                TargetSessionToken = targetToken
            };

            ApplyTargetSession(link);
            return new ImpersonationResult
            {
                Status = ImpersonationStatus.Started,
                ImpersonationId = intent.ImpersonationId
            };
        }

        public static ImpersonationResult CloseCurrent(string endReason)
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
                return ImpersonationResult.Disabled();

            ImpersonationLink link = CurrentLink();
            if (link == null)
                return ExactlyOnceNoOp(null, TryReadUserId(), endReason);

            return Close(link.ImpersonationId, endReason);
        }

        public static ImpersonationResult Close(Guid impersonationId, string endReason)
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
                return ImpersonationResult.Disabled();

            if (impersonationId == Guid.Empty)
                return ImpersonationResult.Fail("ImpersonationId is required.");

            string reason = ImpersonationGovernance.IsKnownEndReason(endReason)
                ? endReason
                : ImpersonationGovernance.EndReason.SystemFault;

            ImpersonationLink link = CurrentLink();
            int? actorId = link != null ? (int?)link.ActorUserId : TryReadUserId();
            LeaseRow lease;

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (SqlTransaction tran = cn.BeginTransaction())
                    {
                        int closed = CloseLeaseOnce(cn, tran, impersonationId, reason);
                        if (closed == 0)
                        {
                            SecurityAudit.Write(cn, tran, actorId, SecurityAudit.ImpersonationCloseNoOp,
                                FormatDetails(impersonationId, actorId, null, TryReadCompanyId(), ImpersonationGovernance.InvExactlyOnceClose));
                            tran.Commit();
                            return new ImpersonationResult
                            {
                                Status = ImpersonationStatus.ClosedNoOp,
                                Invariant = ImpersonationGovernance.InvExactlyOnceClose,
                                ImpersonationId = impersonationId,
                                Message = "Lease already closed."
                            };
                        }

                        if (!TryReadLease(cn, tran, impersonationId, out lease))
                        {
                            tran.Rollback();
                            return ImpersonationResult.Fail("Closed lease could not be read.");
                        }

                        if (lease.TargetSessionToken.HasValue)
                            DeactivateSession(cn, tran, lease.TargetSessionToken.Value);

                        Guid restoredToken = Guid.NewGuid();
                        InsertActiveSession(cn, tran, restoredToken, lease.ActorUserId, lease.CompanyID, ClientIp(), ClientUserAgent());
                        lease.RestoredActorToken = restoredToken;

                        SecurityAudit.Write(cn, tran, lease.ActorUserId, SecurityAudit.ImpersonationClosed,
                            FormatDetails(impersonationId, lease.ActorUserId, lease.TargetUserId, lease.CompanyID, reason));
                        tran.Commit();
                    }
                }
            }
            catch (SqlException)
            {
                return ImpersonationResult.Fail("Close failed.");
            }

            RestoreActorSession(lease, link);
            return new ImpersonationResult
            {
                Status = ImpersonationStatus.Closed,
                Invariant = ImpersonationGovernance.InvExactlyOnceClose,
                ImpersonationId = impersonationId
            };
        }

        public static ImpersonationResult HeartbeatCurrent()
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
                return ImpersonationResult.Disabled();

            ImpersonationLink link = CurrentLink();
            if (link == null)
                return ImpersonationResult.Disabled();

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.ImpersonationSessions
                      SET LastHeartbeat = SYSUTCDATETIME()
                      WHERE ImpersonationId = @Id AND IsActive = 1 AND EndedAt IS NULL
                        AND LeaseExpiresAt > SYSUTCDATETIME()", cn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = link.ImpersonationId;
                    cn.Open();
                    if (cmd.ExecuteNonQuery() == 0)
                        return Close(link.ImpersonationId, ImpersonationGovernance.EndReason.LeaseExpired);
                }
            }
            catch (SqlException)
            {
                return ImpersonationResult.Fail("Heartbeat failed.");
            }

            return new ImpersonationResult { Status = ImpersonationStatus.Started, ImpersonationId = link.ImpersonationId };
        }

        public static void BindMasterBanner(Panel banner, Label text, LinkButton endButton)
        {
            if (banner == null)
                return;

            if (!ImpersonationGovernance.IsSwitchUserEnabled)
            {
                banner.Visible = false;
                return;
            }

            ImpersonationLink link = CurrentLink();
            if (link == null)
            {
                banner.Visible = false;
                return;
            }

            ImpersonationResult expiry = HeartbeatCurrent();
            if (expiry.Status != ImpersonationStatus.Started)
            {
                banner.Visible = false;
                return;
            }

            if (!AuthGuard.HasPermission(ImpersonationGovernance.PermissionKey))
            {
                Close(link.ImpersonationId, ImpersonationGovernance.EndReason.PermissionRevoked);
                banner.Visible = false;
                return;
            }

            banner.Visible = true;
            if (text != null)
            {
                text.Text = "Acting as " + (link.TargetUserKey ?? link.TargetUserId.ToString(CultureInfo.InvariantCulture))
                    + " (rollback restores "
                    + (link.ActorUserKey ?? link.ActorUserId.ToString(CultureInfo.InvariantCulture))
                    + ").";
            }
            if (endButton != null)
                endButton.Visible = true;
        }

        private static ImpersonationResult GateActor(int targetUserId)
        {
            if (!AuthGuard.HasPermission(ImpersonationGovernance.PermissionKey))
                return DenyAndAudit(null, "SwitchUser permission is required.");

            ActorContext actor;
            if (!TryReadActor(out actor))
                return ImpersonationResult.Fail("Actor session is incomplete.");

            if (CurrentLink() != null || HasActiveLease(actor.SessionToken, actor.UserId))
                return DenyAndAudit(ImpersonationGovernance.InvNested, "Nested impersonation is rejected.");

            if (targetUserId <= 0)
                return DenyAndAudit(ImpersonationGovernance.InvCompany, "Target user is required.");

            if (targetUserId == actor.UserId)
                return DenyAndAudit(ImpersonationGovernance.InvSelf, "Self-impersonation is rejected.");

            TargetProfile unused;
            return LoadEligibleTarget(targetUserId, actor.CompanyID, out unused);
        }

        private static ImpersonationResult LoadEligibleTarget(int targetUserId, int companyId, out TargetProfile target)
        {
            target = null;
            const string sql = @"
                SELECT u.Id, u.User_Id, u.RoleId, u.ProfilePictureUrl, r.RoleName
                FROM dbo.tbl_login u
                INNER JOIN dbo.UserCompanyAccess a
                    ON a.UserId = u.Id AND a.CompanyID = @CompanyID AND a.IsActive = 1
                LEFT JOIN dbo.Roles r ON r.RoleId = u.RoleId AND r.CompanyID = @CompanyID
                WHERE u.Id = @TargetUserId
                  AND u.CompanyID = @CompanyID
                  AND u.IsActive = 1
                  AND (u.LockoutEnd IS NULL OR u.LockoutEnd < SYSUTCDATETIME())";

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@TargetUserId", SqlDbType.Int).Value = targetUserId;
                    cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                    cn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.Read())
                            return DenyAndAudit(ImpersonationGovernance.InvCompany, "Target is not eligible in the actor company.");

                        target = new TargetProfile
                        {
                            UserId = Convert.ToInt32(rdr["Id"]),
                            UserKey = rdr["User_Id"] == DBNull.Value ? null : Convert.ToString(rdr["User_Id"]),
                            RoleId = rdr["RoleId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["RoleId"]),
                            RoleName = rdr["RoleName"] == DBNull.Value ? null : Convert.ToString(rdr["RoleName"]),
                            ProfilePic = rdr["ProfilePictureUrl"] == DBNull.Value ? null : Convert.ToString(rdr["ProfilePictureUrl"])
                        };
                    }
                }
            }
            catch (SqlException)
            {
                return DenyAndAudit(ImpersonationGovernance.InvCompany, "Target company membership could not be verified.");
            }

            return null;
        }

        private static bool HasActiveLease(Guid sessionToken, int actorUserId)
        {
            const string sql = @"
                SELECT TOP 1 1
                FROM dbo.ImpersonationSessions
                WHERE IsActive = 1 AND EndedAt IS NULL
                  AND (ActorSessionToken = @Token OR TargetSessionToken = @Token OR ActorUserId = @ActorUserId)";

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = sessionToken;
                    cmd.Parameters.Add("@ActorUserId", SqlDbType.Int).Value = actorUserId;
                    cn.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (SqlException)
            {
                return true;
            }
        }

        private static void InsertLease(SqlConnection cn, SqlTransaction tran, ImpersonationIntent intent, ActorContext actor, Guid targetToken, DateTimeOffset leaseExpires)
        {
            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.ImpersonationSessions
                    (ImpersonationId, CompanyID, ActorUserId, TargetUserId, ActorSessionToken, TargetSessionToken,
                     StartedAt, LastHeartbeat, LeaseExpiresAt, EndedAt, EndReason, IsActive)
                  VALUES
                    (@ImpersonationId, @CompanyID, @ActorUserId, @TargetUserId, @ActorSessionToken, @TargetSessionToken,
                     SYSUTCDATETIME(), SYSUTCDATETIME(), @LeaseExpiresAt, NULL, NULL, 1)", cn, tran))
            {
                cmd.Parameters.Add("@ImpersonationId", SqlDbType.UniqueIdentifier).Value = intent.ImpersonationId;
                cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = actor.CompanyID;
                cmd.Parameters.Add("@ActorUserId", SqlDbType.Int).Value = actor.UserId;
                cmd.Parameters.Add("@TargetUserId", SqlDbType.Int).Value = intent.TargetUserId;
                cmd.Parameters.Add("@ActorSessionToken", SqlDbType.UniqueIdentifier).Value = actor.SessionToken;
                cmd.Parameters.Add("@TargetSessionToken", SqlDbType.UniqueIdentifier).Value = targetToken;
                cmd.Parameters.Add("@LeaseExpiresAt", SqlDbType.DateTimeOffset).Value = leaseExpires;
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertActiveSession(SqlConnection cn, SqlTransaction tran, Guid token, int userId, int companyId, string ip, string ua)
        {
            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.ActiveSessions (SessionToken, UserId, IPAddress, UserAgent, IsActive, CompanyID)
                  VALUES (@Token, @UserId, @IP, @UA, 1, @CompanyID)", cn, tran))
            {
                cmd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = token;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@IP", SqlDbType.NVarChar, 50).Value = (object)ip ?? DBNull.Value;
                cmd.Parameters.Add("@UA", SqlDbType.NVarChar, 500).Value = (object)ua ?? DBNull.Value;
                cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                cmd.ExecuteNonQuery();
            }
        }

        private static void DeactivateSession(SqlConnection cn, SqlTransaction tran, Guid token)
        {
            using (var cmd = new SqlCommand(
                "UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE SessionToken = @Token", cn, tran))
            {
                cmd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = token;
                cmd.ExecuteNonQuery();
            }
        }

        private static int CloseLeaseOnce(SqlConnection cn, SqlTransaction tran, Guid impersonationId, string endReason)
        {
            using (var cmd = new SqlCommand(
                @"UPDATE dbo.ImpersonationSessions
                  SET IsActive = 0, EndedAt = SYSUTCDATETIME(), EndReason = @EndReason
                  WHERE ImpersonationId = @Id AND IsActive = 1 AND EndedAt IS NULL", cn, tran))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = impersonationId;
                cmd.Parameters.Add("@EndReason", SqlDbType.NVarChar, 32).Value = endReason;
                return cmd.ExecuteNonQuery();
            }
        }

        private static bool TryReadLease(SqlConnection cn, SqlTransaction tran, Guid impersonationId, out LeaseRow lease)
        {
            lease = null;
            using (var cmd = new SqlCommand(
                @"SELECT CompanyID, ActorUserId, TargetUserId, TargetSessionToken
                  FROM dbo.ImpersonationSessions WHERE ImpersonationId = @Id", cn, tran))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = impersonationId;
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                        return false;
                    lease = new LeaseRow
                    {
                        ImpersonationId = impersonationId,
                        CompanyID = Convert.ToInt32(rdr["CompanyID"]),
                        ActorUserId = Convert.ToInt32(rdr["ActorUserId"]),
                        TargetUserId = Convert.ToInt32(rdr["TargetUserId"]),
                        TargetSessionToken = rdr["TargetSessionToken"] == DBNull.Value ? (Guid?)null : (Guid)rdr["TargetSessionToken"]
                    };
                    return true;
                }
            }
        }

        private static ImpersonationResult ExactlyOnceNoOp(Guid? impersonationId, int? userId, string endReason)
        {
            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (SqlTransaction tran = cn.BeginTransaction())
                    {
                        SecurityAudit.Write(cn, tran, userId, SecurityAudit.ImpersonationCloseNoOp,
                            FormatDetails(impersonationId, userId, null, TryReadCompanyId(), ImpersonationGovernance.InvExactlyOnceClose + ":" + (endReason ?? "")));
                        tran.Commit();
                    }
                }
            }
            catch (SqlException)
            {
                // No session mutation either way.
            }

            return new ImpersonationResult
            {
                Status = ImpersonationStatus.ClosedNoOp,
                Invariant = ImpersonationGovernance.InvExactlyOnceClose,
                ImpersonationId = impersonationId,
                Message = "No active lease."
            };
        }

        private static ImpersonationResult DenyAndAudit(string invariant, string message)
        {
            int? userId = TryReadUserId();
            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (SqlTransaction tran = cn.BeginTransaction())
                    {
                        SecurityAudit.Write(cn, tran, userId, SecurityAudit.ImpersonationStartDenied,
                            (invariant ?? "") + ":" + message);
                        tran.Commit();
                    }
                }
            }
            catch (SqlException)
            {
                return ImpersonationResult.Fail("Denial audit failed.");
            }

            return ImpersonationResult.Deny(invariant, message);
        }

        private static bool TryReadActor(out ActorContext actor)
        {
            actor = null;
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null)
                return false;

            int userId;
            int companyId;
            Guid token;
            if (!TryReadUserId(out userId))
                return false;
            if (!TryReadCompanyId(out companyId))
                return false;
            if (ctx.Session["SessionToken"] == null || !Guid.TryParse(Convert.ToString(ctx.Session["SessionToken"]), out token))
                return false;
            if (ctx.Session["USERID"] == null)
                return false;

            int roleId;
            int? role = null;
            if (ctx.Session["RoleId"] != null && int.TryParse(Convert.ToString(ctx.Session["RoleId"]), out roleId))
                role = roleId;

            actor = new ActorContext
            {
                UserId = userId,
                UserKey = Convert.ToString(ctx.Session["USERID"]),
                CompanyID = companyId,
                SessionToken = token,
                RoleId = role,
                RoleName = ctx.Session["RoleName"] == null ? null : Convert.ToString(ctx.Session["RoleName"]),
                UserType = ctx.Session["USERTYPE"] == null ? null : Convert.ToString(ctx.Session["USERTYPE"]),
                ProfilePic = ctx.Session["ProfilePic"] == null ? null : Convert.ToString(ctx.Session["ProfilePic"])
            };
            return true;
        }

        private static void ApplyTargetSession(ImpersonationLink link)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null)
                return;

            ctx.Session[ImpersonationGovernance.SessionLinkKey] = link;
            ctx.Session["USERID"] = link.TargetUserKey;
            ctx.Session["UserDbId"] = link.TargetUserId;
            ctx.Session["RoleId"] = link.TargetRoleId.HasValue ? (object)link.TargetRoleId.Value : null;
            ctx.Session["RoleName"] = string.IsNullOrEmpty(link.TargetRoleName) ? "Standard User" : link.TargetRoleName;
            ctx.Session["ProfilePic"] = link.TargetProfilePic;
            ctx.Session["SessionToken"] = link.TargetSessionToken;
        }

        private static void RestoreActorSession(LeaseRow lease, ImpersonationLink link)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null)
                return;

            ctx.Session.Remove(ImpersonationGovernance.SessionLinkKey);
            ctx.Session["SessionToken"] = lease.RestoredActorToken;
            ctx.Session["UserDbId"] = lease.ActorUserId;

            if (link != null && link.ImpersonationId == lease.ImpersonationId)
            {
                ctx.Session["USERID"] = link.ActorUserKey;
                ctx.Session["RoleId"] = link.ActorRoleId.HasValue ? (object)link.ActorRoleId.Value : null;
                ctx.Session["RoleName"] = string.IsNullOrEmpty(link.ActorRoleName) ? "Standard User" : link.ActorRoleName;
                ctx.Session["ProfilePic"] = link.ActorProfilePic;
                ctx.Session["USERTYPE"] = link.ActorUserType;
                return;
            }

            ctx.Session["USERID"] = LoadUserKey(lease.ActorUserId);
        }

        private static string LoadUserKey(int userId)
        {
            const string sql = "SELECT User_Id FROM dbo.tbl_login WHERE Id = @Id";
            using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = userId;
                cn.Open();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? null : Convert.ToString(value);
            }
        }

        private static ImpersonationLink CurrentLink()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null)
                return null;
            return ctx.Session[ImpersonationGovernance.SessionLinkKey] as ImpersonationLink;
        }

        private static int? TryReadUserId()
        {
            int id;
            return TryReadUserId(out id) ? (int?)id : null;
        }

        private static bool TryReadUserId(out int userId)
        {
            userId = 0;
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null || ctx.Session["UserDbId"] == null)
                return false;
            return int.TryParse(Convert.ToString(ctx.Session["UserDbId"]), out userId) && userId > 0;
        }

        private static int? TryReadCompanyId()
        {
            int id;
            return TryReadCompanyId(out id) ? (int?)id : null;
        }

        private static bool TryReadCompanyId(out int companyId)
        {
            companyId = CompanyContext.CurrentCompanyID;
            return companyId > 0;
        }

        private static string FormatDetails(Guid? impersonationId, int? actorId, int? targetId, int? companyId, string note)
        {
            return "id=" + (impersonationId.HasValue ? impersonationId.Value.ToString("N") : "")
                + ";actor=" + (actorId.HasValue ? actorId.Value.ToString(CultureInfo.InvariantCulture) : "")
                + ";target=" + (targetId.HasValue ? targetId.Value.ToString(CultureInfo.InvariantCulture) : "")
                + ";company=" + (companyId.HasValue ? companyId.Value.ToString(CultureInfo.InvariantCulture) : "")
                + ";note=" + (note ?? "");
        }

        private static bool IsUniqueViolation(SqlException ex)
        {
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                if (ex.Errors[i].Number == 2627 || ex.Errors[i].Number == 2601)
                    return true;
            }
            return false;
        }

        private static string ClientIp()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Request == null)
                return null;
            string forwarded = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(forwarded))
            {
                int comma = forwarded.IndexOf(',');
                string first = (comma >= 0 ? forwarded.Substring(0, comma) : forwarded).Trim();
                return first.Length > 50 ? first.Substring(0, 50) : first;
            }
            string remote = ctx.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(remote))
                return null;
            return remote.Length > 50 ? remote.Substring(0, 50) : remote;
        }

        private static string ClientUserAgent()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Request == null || ctx.Request.UserAgent == null)
                return null;
            string ua = ctx.Request.UserAgent;
            return ua.Length > 500 ? ua.Substring(0, 500) : ua;
        }

        private sealed class ActorContext
        {
            public int UserId;
            public string UserKey;
            public int CompanyID;
            public Guid SessionToken;
            public int? RoleId;
            public string RoleName;
            public string UserType;
            public string ProfilePic;
        }

        private sealed class TargetProfile
        {
            public int UserId;
            public string UserKey;
            public int? RoleId;
            public string RoleName;
            public string ProfilePic;
        }

        private sealed class LeaseRow
        {
            public Guid ImpersonationId;
            public int CompanyID;
            public int ActorUserId;
            public int TargetUserId;
            public Guid? TargetSessionToken;
            public Guid RestoredActorToken;
        }
    }
}
