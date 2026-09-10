using System;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// PR-2A governance contracts for administrator impersonation.
    /// Does not swap identity, write ImpersonationSessions, start a heartbeat,
    /// render a banner, or grant permissions. AuthGuard is unchanged.
    /// See docs/ADR-001_Administrator_Impersonation.md and docs/29–33.
    /// </summary>
    public static class ImpersonationGovernance
    {
        public const string PermissionKey = "SwitchUser";
        public const string FeatureFlagKey = "SwitchUser";
        public const string LedgerTable = "dbo.ImpersonationSessions";

        /// <summary>
        /// Fail closed. Missing, empty, or any value other than "true" is disabled.
        /// PR-2A does not call this from request gates.
        /// </summary>
        public static bool IsSwitchUserEnabled
        {
            get
            {
                string raw = AppSecrets.GetAppSetting(FeatureFlagKey);
                if (string.IsNullOrWhiteSpace(raw))
                    return false;
                return string.Equals(raw.Trim(), "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Canonical EndReason values. Must match CK_ImpersonationSessions_EndReason
        /// in db/impersonation_pr1.sql. Runtime writers belong to later PRs.
        /// </summary>
        public static class EndReason
        {
            public const string ManualRollback = "ManualRollback";
            public const string ActorLogout = "ActorLogout";
            public const string TargetLogout = "TargetLogout";
            public const string IdleTimeout = "IdleTimeout";
            public const string HeartbeatMissed = "HeartbeatMissed";
            public const string LeaseExpired = "LeaseExpired";
            public const string ActorSessionKilled = "ActorSessionKilled";
            public const string TargetSessionKilled = "TargetSessionKilled";
            public const string ForcedRevoke = "ForcedRevoke";
            public const string PermissionRevoked = "PermissionRevoked";
            public const string SystemFault = "SystemFault";
        }
    }
}
