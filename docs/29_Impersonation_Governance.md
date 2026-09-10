# 29 — Impersonation governance

**Status:** Governance + runtime. Live swap still requires UAT flag + Super Admin grant.  
**ADR:** [ADR-001_Administrator_Impersonation.md](ADR-001_Administrator_Impersonation.md)

PR-83 callers: [36_Impersonation_UAT_Activation.md](36_Impersonation_UAT_Activation.md). `AuthGuard` and `Heartbeat.ashx` stay unchanged. Production `SwitchUser` remains false.
