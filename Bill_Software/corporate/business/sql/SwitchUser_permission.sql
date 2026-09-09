-- =============================================================================
-- SwitchUser Permission — PR-1 neutralized
-- =============================================================================
-- When: PR-1 impersonation stabilization.
-- Why:  The unshipped Switch User prototype shipped a catalog insert for
--       PermissionKey = 'SwitchUser'. PR-1 must not activate impersonation
--       and must not grant that key.
-- What: No INSERT into dbo.Permissions. No INSERT into dbo.RolePermissions.
--       Re-running this script is a no-op. Do not grant SwitchUser until a
--       later PR that explicitly activates impersonation.
-- =============================================================================

PRINT 'PR-1: SwitchUser permission is not created or granted. Impersonation is not activated.';
GO
