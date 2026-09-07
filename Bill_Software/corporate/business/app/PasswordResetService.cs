using System;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public static class PasswordResetService
    {
        public static int TokenLifetimeMinutes
        {
            get { return AppSecrets.GetInt("PasswordResetTokenMinutes", 30); }
        }

        public static bool TryIssueToken(int userDbId, string userId, out string rawToken, out string error)
        {
            rawToken = null;
            error = null;

            if (userDbId <= 0 || string.IsNullOrWhiteSpace(userId))
            {
                error = "Unable to process the reset request.";
                return false;
            }

            string token = CryptoRandom.GenerateHexToken(32);
            string tokenHash = PasswordHasher.Sha256Hex(token);
            DateTime expires = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes);

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (var cmdInv = new SqlCommand(
                        @"UPDATE dbo.PasswordResetTokens
                          SET UsedAtUtc = SYSUTCDATETIME()
                          WHERE UserDbId = @UserDbId AND UsedAtUtc IS NULL", cn))
                    {
                        cmdInv.Parameters.AddWithValue("@UserDbId", userDbId);
                        cmdInv.ExecuteNonQuery();
                    }

                    using (var cmdIns = new SqlCommand(
                        @"INSERT INTO dbo.PasswordResetTokens (UserDbId, UserId, TokenHash, ExpiresAtUtc)
                          VALUES (@UserDbId, @UserId, @TokenHash, @ExpiresAtUtc)", cn))
                    {
                        cmdIns.Parameters.AddWithValue("@UserDbId", userDbId);
                        cmdIns.Parameters.AddWithValue("@UserId", userId);
                        cmdIns.Parameters.AddWithValue("@TokenHash", tokenHash);
                        cmdIns.Parameters.AddWithValue("@ExpiresAtUtc", expires);
                        cmdIns.ExecuteNonQuery();
                    }
                }

                rawToken = token;
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 208)
                {
                    error = "Password reset is temporarily unavailable. Please contact your administrator.";
                    return false;
                }
                throw;
            }
        }

        public static bool TryCompleteReset(string userId, string rawToken, string newPassword, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(rawToken) || string.IsNullOrEmpty(newPassword))
            {
                error = "Invalid or expired reset token.";
                return false;
            }

            string submittedHash = PasswordHasher.Sha256Hex(rawToken.Trim());

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (var tran = cn.BeginTransaction())
                    {
                        int tokenId;
                        int userDbId;
                        string storedHash;
                        DateTime expiresAt;

                        using (var cmd = new SqlCommand(
                            @"SELECT TOP 1 TokenId, TokenHash, ExpiresAtUtc, UserDbId
                              FROM dbo.PasswordResetTokens
                              WHERE UserId = @UserId AND UsedAtUtc IS NULL
                              ORDER BY TokenId DESC", cn, tran))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId.Trim());
                            using (SqlDataReader rdr = cmd.ExecuteReader())
                            {
                                if (!rdr.Read())
                                {
                                    error = "Invalid or expired reset token.";
                                    return false;
                                }

                                tokenId = Convert.ToInt32(rdr["TokenId"]);
                                storedHash = rdr["TokenHash"] != DBNull.Value ? rdr["TokenHash"].ToString() : string.Empty;
                                expiresAt = Convert.ToDateTime(rdr["ExpiresAtUtc"]);
                                userDbId = Convert.ToInt32(rdr["UserDbId"]);
                            }
                        }

                        if (DateTime.UtcNow > expiresAt || !CryptoRandom.FixedTimeEquals(storedHash, submittedHash))
                        {
                            error = "Invalid or expired reset token.";
                            return false;
                        }

                        using (var cmdUser = new SqlCommand(
                            @"SELECT IsActive FROM tbl_login WHERE Id = @Id AND User_Id = @UserId", cn, tran))
                        {
                            cmdUser.Parameters.AddWithValue("@Id", userDbId);
                            cmdUser.Parameters.AddWithValue("@UserId", userId.Trim());
                            object active = cmdUser.ExecuteScalar();
                            if (active == null || active == DBNull.Value || !Convert.ToBoolean(active))
                            {
                                error = "Invalid or expired reset token.";
                                return false;
                            }
                        }

                        using (var cmdUse = new SqlCommand(
                            @"UPDATE dbo.PasswordResetTokens
                              SET UsedAtUtc = SYSUTCDATETIME()
                              WHERE TokenId = @TokenId
                                AND UsedAtUtc IS NULL
                                AND ExpiresAtUtc > SYSUTCDATETIME()", cn, tran))
                        {
                            cmdUse.Parameters.AddWithValue("@TokenId", tokenId);
                            if (cmdUse.ExecuteNonQuery() != 1)
                            {
                                error = "Invalid or expired reset token.";
                                return false;
                            }
                        }

                        byte[] hash;
                        byte[] salt;
                        PasswordHasher.Create(newPassword, out hash, out salt);

                        using (var cmdPwd = new SqlCommand(
                            @"UPDATE tbl_login
                              SET PasswordHash = @Hash,
                                  PasswordSalt = @Salt,
                                  Password = NULL,
                                  MustChangePassword = 0,
                                  FailedAccessCount = 0,
                                  LockoutEnd = NULL
                              WHERE Id = @Id AND User_Id = @UserId AND IsActive = 1", cn, tran))
                        {
                            cmdPwd.Parameters.Add("@Hash", SqlDbType.VarBinary, PasswordHasher.HashSize).Value = hash;
                            cmdPwd.Parameters.Add("@Salt", SqlDbType.VarBinary, PasswordHasher.SaltSize).Value = salt;
                            cmdPwd.Parameters.AddWithValue("@Id", userDbId);
                            cmdPwd.Parameters.AddWithValue("@UserId", userId.Trim());
                            if (cmdPwd.ExecuteNonQuery() != 1)
                            {
                                error = "Unable to update the password. Please contact your administrator.";
                                return false;
                            }
                        }

                        tran.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 208)
                {
                    error = "Password reset is temporarily unavailable. Please contact your administrator.";
                    return false;
                }
                throw;
            }
        }
    }
}
