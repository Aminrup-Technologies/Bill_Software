using System;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Impersonation contracts. Runtime writers live in ImpersonationRuntime
    /// and execute only when IsSwitchUserEnabled is true. AuthGuard is unchanged.
    /// See docs/ADR-001 and docs/29–35.
    /// </summary>
    public static class ImpersonationGovernance
    {
        public const string PermissionKey = "SwitchUser";
        public const string FeatureFlagKey = "SwitchUser";
        public const string LedgerTable = "dbo.ImpersonationSessions";
        public const string SessionLinkKey = "ImpersonationLink";

        public const int IntentTtlMinutes = 2;
        public const int LeaseDurationMinutes = 15;

        public const string InvNested = "INV-13";
        public const string InvSelf = "INV-14";
        public const string InvCompany = "INV-15";
        public const string InvIntent = "INV-16";
        public const string InvExactlyOnceClose = "INV-17";

        /// <summary>
        /// Fail closed. Missing, empty, or any value other than "true" is disabled.
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

        public static bool IsKnownEndReason(string endReason)
        {
            if (string.IsNullOrEmpty(endReason))
                return false;
            return endReason == EndReason.ManualRollback
                || endReason == EndReason.ActorLogout
                || endReason == EndReason.TargetLogout
                || endReason == EndReason.IdleTimeout
                || endReason == EndReason.HeartbeatMissed
                || endReason == EndReason.LeaseExpired
                || endReason == EndReason.ActorSessionKilled
                || endReason == EndReason.TargetSessionKilled
                || endReason == EndReason.ForcedRevoke
                || endReason == EndReason.PermissionRevoked
                || endReason == EndReason.SystemFault;
        }

        /// <summary>
        /// Canonical EndReason values. Must match CK_ImpersonationSessions_EndReason
        /// in db/impersonation_pr1.sql. Writers run only when SwitchUser is true.
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
