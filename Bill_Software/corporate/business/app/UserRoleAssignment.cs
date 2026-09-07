using System;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Keeps tbl_login.RoleId (display/primary) and dbo.UserRoles (permission grants)
    /// from drifting. UserRoles stays many-to-many. Does not read or write Permissions.
    /// </summary>
    public static class UserRoleAssignment
    {
        public static bool RoleBelongsToCompany(SqlConnection cn, SqlTransaction tran, int roleId, int companyId)
        {
            if (cn == null || roleId <= 0 || companyId <= 0)
                return false;

            using (var cmd = new SqlCommand(
                "SELECT TOP 1 1 FROM dbo.Roles WHERE RoleId = @RoleId AND CompanyID = @CompanyID", cn, tran))
            {
                cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                return cmd.ExecuteScalar() != null;
            }
        }

        public static bool MappingExists(SqlConnection cn, SqlTransaction tran, int userDbId, int roleId)
        {
            if (cn == null || userDbId <= 0 || roleId <= 0)
                return false;

            using (var cmd = new SqlCommand(
                "SELECT TOP 1 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId", cn, tran))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userDbId;
                cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                return cmd.ExecuteScalar() != null;
            }
        }

        /// <summary>
        /// Insert (UserId, RoleId) when missing. Role must belong to the company.
        /// Returns false without inserting when inputs are invalid.
        /// </summary>
        public static bool TryEnsureMapping(SqlConnection cn, SqlTransaction tran, int userDbId, int roleId, int companyId)
        {
            if (cn == null || userDbId <= 0 || roleId <= 0 || companyId <= 0)
                return false;
            if (!RoleBelongsToCompany(cn, tran, roleId, companyId))
                return false;
            if (MappingExists(cn, tran, userDbId, roleId))
                return true;

            using (var cmd = new SqlCommand(
                "INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", cn, tran))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userDbId;
                cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        /// <summary>
        /// View User changed the single display RoleId. Ensure the new role is in
        /// UserRoles. Remove only the previous display role. Extra Update_Designation
        /// mappings are left in place.
        /// </summary>
        public static bool TrySyncDisplayRoleChange(SqlConnection cn, SqlTransaction tran, int userDbId, int? previousRoleId, int? newRoleId, int companyId)
        {
            if (cn == null || userDbId <= 0 || companyId <= 0)
                return false;

            if (!newRoleId.HasValue || newRoleId.Value <= 0)
                return true;

            if (!TryEnsureMapping(cn, tran, userDbId, newRoleId.Value, companyId))
                return false;

            if (previousRoleId.HasValue && previousRoleId.Value > 0 && previousRoleId.Value != newRoleId.Value)
            {
                using (var cmd = new SqlCommand(
                    "DELETE FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @OldRoleId", cn, tran))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userDbId;
                    cmd.Parameters.Add("@OldRoleId", SqlDbType.Int).Value = previousRoleId.Value;
                    cmd.ExecuteNonQuery();
                }
            }

            return MappingExists(cn, tran, userDbId, newRoleId.Value);
        }

        /// <summary>
        /// After Update_Designation rewrites UserRoles, keep tbl_login.RoleId as a
        /// role that still exists in UserRoles, or the lowest remaining RoleId.
        /// </summary>
        public static void AlignDisplayRoleFromUserRoles(SqlConnection cn, SqlTransaction tran, int userDbId, int companyId)
        {
            if (cn == null || userDbId <= 0 || companyId <= 0)
                return;

            using (var cmd = new SqlCommand(
                @"UPDATE dbo.tbl_login
                  SET RoleId = COALESCE(
                        CASE WHEN EXISTS (
                            SELECT 1 FROM dbo.UserRoles ur
                            WHERE ur.UserId = tbl_login.Id AND ur.RoleId = tbl_login.RoleId)
                        THEN tbl_login.RoleId END,
                        (SELECT MIN(ur.RoleId) FROM dbo.UserRoles ur WHERE ur.UserId = tbl_login.Id)
                    )
                  WHERE Id = @UserId AND CompanyID = @CompanyID", cn, tran))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userDbId;
                cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
