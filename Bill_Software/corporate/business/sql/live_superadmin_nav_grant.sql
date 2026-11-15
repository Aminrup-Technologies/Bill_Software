/* **************************************************************************
   NAME:        live_superadmin_nav_grant
   WHEN:        2026-11-15
   WHY:         LIVE Super Admin (resolved via UserRoles for tbl_login.Id=1 /
                User_Id=N'admin') must receive RolePermissions for every
                PermissionKey required by the 113 primary-navigation ERP pages
                (PAGE_INVENTORY menu rows + AdminAttendanceDashboard /
                AdminShiftAssignment Bill.Master ids). Catalog lacks
                AdminAttendanceDashboard on some hosts; insert that one key by
                PermissionKey only. Pattern follows
                SwitchUser_live_catalog_grant_proposal.sql.
   WHAT:        On DB_NAME()=N'flamex_live' only: (1) validate IDENTITY /
                PermissionKey uniqueness / admin / Super Admin role;
                (2) idempotent INSERT dbo.Permissions for
                AdminAttendanceDashboard if absent (Dashboard / Employee Config /
                Attendance Register); (3) idempotent INSERT dbo.RolePermissions
                for the approved Super Admin role for all listed nav keys that
                already exist in dbo.Permissions (join by PermissionKey — never
                hardcode PermissionId). No UserRoles changes. No deletes of
                unrelated rows. DBA-executed only; never run from the app.
   ************************************************************************** */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- Separate written approval required before any flamex_live execution.
-- Does not grant catalog keys outside the 113-page derived set.
-- Does not INSERT missing keys other than AdminAttendanceDashboard.
-- Bill.Master leaves AdminHolidaySetup / AdminLeaveBalance / SwitchUser are
-- outside the 113-page inventory census and are intentionally omitted here.

SET NOCOUNT ON;
SET XACT_ABORT ON;

-----------------------------------------------------------------------------
-- 0. Database guard
-----------------------------------------------------------------------------
IF DB_NAME() <> N'flamex_live'
BEGIN
    RAISERROR('live_superadmin_nav_grant.sql requires DB_NAME() = N''flamex_live''. No changes applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.tbl_login', N'U') IS NULL
BEGIN
    RAISERROR('Required RBAC tables missing. No changes applied.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 1. Schema preflight: PermissionId IDENTITY + PermissionKey uniqueness
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.Permissions')
      AND c.name = N'PermissionId'
      AND c.is_identity = 1
)
BEGIN
    RAISERROR('Precondition failed: dbo.Permissions.PermissionId is not IDENTITY. Aborting (do not force PermissionId).', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(N'dbo.Permissions')
      AND i.is_unique = 1
      AND EXISTS (
            SELECT 1
            FROM sys.index_columns ic
            INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE ic.object_id = i.object_id
              AND ic.index_id = i.index_id
              AND c.name = N'PermissionKey'
        )
)
BEGIN
    RAISERROR('Precondition failed: no unique index/constraint found on dbo.Permissions.PermissionKey. Aborting.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 2. Resolve intended admin + Super Admin role (no hardcoded user grants)
-----------------------------------------------------------------------------
DECLARE @AdminUserId int;
DECLARE @AdminLogin nvarchar(100);
DECLARE @AdminCompanyID int;
DECLARE @RoleId int;
DECLARE @RoleName nvarchar(100);
DECLARE @RoleCompanyID int;

SELECT
    @AdminUserId = u.Id,
    @AdminLogin = u.User_Id,
    @AdminCompanyID = u.CompanyID
FROM dbo.tbl_login u
WHERE u.Id = 1
  AND u.User_Id = N'admin';

IF @AdminUserId IS NULL OR @AdminLogin IS NULL
BEGIN
    RAISERROR('Precondition failed: tbl_login.Id=1 / User_Id=N''admin'' not found. No changes applied.', 16, 1);
    RETURN;
END;

IF @AdminCompanyID IS NULL OR @AdminCompanyID <> 1
BEGIN
    RAISERROR('Precondition failed: admin CompanyID is not 1 (Flame-Ex). No changes applied.', 16, 1);
    RETURN;
END;

-- Prefer Super Admin RoleId 1 / CompanyID 1; verify name.
SELECT
    @RoleId = r.RoleId,
    @RoleName = r.RoleName,
    @RoleCompanyID = r.CompanyID
FROM dbo.Roles r
WHERE r.RoleId = 1
  AND r.RoleName = N'Super Admin'
  AND r.CompanyID = 1;

IF @RoleId IS NULL
BEGIN
    RAISERROR('Precondition failed: Roles RoleId=1 Super Admin CompanyID=1 missing. No changes applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoles ur
    WHERE ur.UserId = @AdminUserId
      AND ur.RoleId = @RoleId
)
BEGIN
    RAISERROR('Precondition failed: UserRoles does not map admin to Super Admin RoleId. UserRoles are not modified by this script.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 3. Target PermissionKey list (113 primary-nav pages → derived keys)
-----------------------------------------------------------------------------
IF OBJECT_ID(N'tempdb..#TargetKeys') IS NOT NULL DROP TABLE #TargetKeys;
CREATE TABLE #TargetKeys
(
    PermissionKey nvarchar(200) NOT NULL PRIMARY KEY
);

INSERT INTO #TargetKeys (PermissionKey)
VALUES
    (N'AddFactory'),
    (N'AddIndustry'),
    (N'AddPrimaryService'),
    (N'AddUser'),
    (N'Add_DirectInvoice'),
    (N'Add_invoice'),
    (N'Add_proforma'),
    (N'AdminApprovalDashboard'),
    (N'AdminAttendanceDashboard'),
    (N'AdminLeaveSetup'),
    (N'AdminOverride'),
    (N'AdminShiftAssignment'),
    (N'AdminShiftSetup'),
    (N'Block_invoice'),
    (N'CreateHydrentQuatation'),
    (N'Create_quotation'),
    (N'DeleteHydrentQuatation'),
    (N'Delete_Quotation'),
    (N'Delete_chalan'),
    (N'Delete_client'),
    (N'Delete_general_expencess'),
    (N'Delete_invoice'),
    (N'Delete_patty_cash_expenses'),
    (N'Delete_payment'),
    (N'Delete_proforma'),
    (N'Delete_purches_payment'),
    (N'Delete_purtches'),
    (N'Delete_vendor'),
    (N'Deletehydrent_invoice'),
    (N'Edit_Purches'),
    (N'Edit_quatation'),
    (N'Expenses_Head'),
    (N'FinalPaymentInvoice'),
    (N'InvoiceMail'),
    (N'Li2'),
    (N'Li3'),
    (N'Li4'),
    (N'Li5'),
    (N'ManagePermissions'),
    (N'ManageRoles'),
    (N'Manual_proforma'),
    (N'New_client'),
    (N'New_vendor'),
    (N'PaymentMail'),
    (N'PaymentPhase'),
    (N'Payment_due'),
    (N'PaymentsDue'),
    (N'PrimaryServiceTerms'),
    (N'ProformaMail'),
    (N'Purches_exting_vendor'),
    (N'Purchess_due'),
    (N'Representative'),
    (N'RequisitionManual'),
    (N'RequisitionManualDelete'),
    (N'RequisitionManualSearch'),
    (N'RequisitionManualView'),
    (N'SearchHydrentQuatation'),
    (N'Search_Products'),
    (N'Searchhydrent_invoice'),
    (N'Seartch_proforma'),
    (N'Seartch_purchess_payments'),
    (N'Seartch_quotation'),
    (N'Service_Tax_Master'),
    (N'Service_master'),
    (N'Service_stock'),
    (N'SetQuatation'),
    (N'Vat_master'),
    (N'ViewHydrentQuatation'),
    (N'ViewUser'),
    (N'View_Invoice'),
    (N'View_chalan'),
    (N'View_client'),
    (N'View_payment'),
    (N'View_po'),
    (N'View_proforma'),
    (N'View_purches'),
    (N'View_purchess_payment'),
    (N'View_quotation'),
    (N'View_vendor'),
    (N'Viewhydrent_invoice'),
    (N'add_chalan'),
    (N'add_payment'),
    (N'add_payment_purchess'),
    (N'daily_attendance'),
    (N'daily_reporting'),
    (N'general_expences'),
    (N'home1'),
    (N'hydrent_invoice'),
    (N'master_State'),
    (N'master_city'),
    (N'my_leaves'),
    (N'newproduct_master'),
    (N'newproductparent'),
    (N'patty_cash_expences'),
    (N'po_generate'),
    (N'po_view'),
    (N'pr_approve'),
    (N'pr_create'),
    (N'pr_view'),
    (N'product_master'),
    (N'product_stock'),
    (N'productparent'),
    (N'rpts_vw_qtn_po_counts'),
    (N'seartch_chalan'),
    (N'seartch_invoice'),
    (N'seartch_payment'),
    (N'seartch_purtch'),
    (N'settings'),
    (N'srch_dailyrpts'),
    (N'uploader'),
    (N'view_expencess_head'),
    (N'view_patty_cash_expenses'),
    (N'visit_planner'),
    (N'vw_dailyrpts');

DECLARE @TargetKeyCount int = (SELECT COUNT(*) FROM #TargetKeys);

IF @TargetKeyCount <> 114
BEGIN
    RAISERROR('Precondition failed: #TargetKeys must contain exactly 114 unique PermissionKeys (got %d). Aborting.', 16, 1, @TargetKeyCount);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 4. Pre-change snapshots (auditable)
-----------------------------------------------------------------------------
SELECT
    N'PRE' AS SnapshotPhase,
    DB_NAME() AS DbName,
    @AdminUserId AS AdminUserId,
    @AdminLogin AS AdminLogin,
    @AdminCompanyID AS AdminCompanyID,
    @RoleId AS SuperAdminRoleId,
    @RoleName AS SuperAdminRoleName,
    @RoleCompanyID AS SuperAdminRoleCompanyID,
    @TargetKeyCount AS TargetKeyCount,
    (SELECT COUNT_BIG(*) FROM dbo.RolePermissions WHERE RoleId = @RoleId) AS SuperAdminRolePermissionCount,
    (SELECT COUNT_BIG(*) FROM dbo.Permissions WHERE PermissionKey = N'AdminAttendanceDashboard') AS CatalogAadCount,
    (SELECT COUNT_BIG(*)
     FROM dbo.RolePermissions rp
     INNER JOIN dbo.Permissions p ON p.PermissionId = rp.PermissionId
     WHERE rp.RoleId = @RoleId AND p.PermissionKey = N'AdminAttendanceDashboard') AS SuperAdminAadGrantCount;

-- Keys required by nav that are absent from LIVE catalog (informational; not inserted except AAD)
SELECT N'MISSING_CATALOG' AS Finding, t.PermissionKey
FROM #TargetKeys t
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Permissions p WHERE p.PermissionKey = t.PermissionKey
)
ORDER BY t.PermissionKey;

-- Existing AAD metadata conflict check (abort if present but wrong module path)
IF EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionKey = N'AdminAttendanceDashboard'
      AND (
            ISNULL(ModuleName, N'') <> N'Dashboard'
         OR ISNULL(SubModuleName, N'') <> N'Employee Config'
         OR ISNULL(FeatureName, N'') <> N'Attendance Register'
      )
)
BEGIN
    RAISERROR('Precondition failed: AdminAttendanceDashboard exists with unexpected Module/SubModule/Feature metadata. Aborting.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 5. Transaction: catalog (AAD only) + RolePermissions for existing keys
-----------------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    -- 5a. Catalog: AdminAttendanceDashboard only (PermissionId via IDENTITY)
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminAttendanceDashboard'
    )
    BEGIN
        INSERT INTO dbo.Permissions
            (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
        VALUES
            (
                N'AdminAttendanceDashboard',
                N'Enterprise attendance register (AdminAttendanceDashboard.aspx). Catalog only; not granted.',
                N'Dashboard',
                N'Employee Config',
                N'Attendance Register'
            );
    END;

    DECLARE @AadPermissionId int =
    (
        SELECT p.PermissionId
        FROM dbo.Permissions p
        WHERE p.PermissionKey = N'AdminAttendanceDashboard'
    );

    IF @AadPermissionId IS NULL
    BEGIN
        THROW 50001, 'AdminAttendanceDashboard catalog row missing after insert attempt.', 1;
    END;

    -- 5b. Grant: only target keys that exist in catalog; resolve PermissionId by key
    DECLARE @InsertedGrants int = 0;

    INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
    SELECT @RoleId, p.PermissionId
    FROM #TargetKeys t
    INNER JOIN dbo.Permissions p ON p.PermissionKey = t.PermissionKey
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.RolePermissions rp
        WHERE rp.RoleId = @RoleId
          AND rp.PermissionId = p.PermissionId
    );

    SET @InsertedGrants = @@ROWCOUNT;

    -----------------------------------------------------------------------------
    -- 6. Postconditions
    -----------------------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Permissions
        WHERE PermissionKey = N'AdminAttendanceDashboard'
          AND PermissionId = @AadPermissionId
          AND ModuleName = N'Dashboard'
          AND SubModuleName = N'Employee Config'
          AND FeatureName = N'Attendance Register'
    )
    BEGIN
        THROW 50002, 'Postcondition failed: AdminAttendanceDashboard catalog metadata mismatch.', 1;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.RolePermissions
        WHERE RoleId = @RoleId
          AND PermissionId = @AadPermissionId
    )
    BEGIN
        THROW 50003, 'Postcondition failed: Super Admin missing AdminAttendanceDashboard RolePermissions row.', 1;
    END;

    -- AuthGuard-shaped lookup for admin @ CompanyID 1
    IF NOT EXISTS (
        SELECT TOP 1 1
        FROM dbo.Permissions p
        INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
        INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
        INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id AND u.CompanyID = @AdminCompanyID
        WHERE u.Id = @AdminUserId
          AND u.User_Id = @AdminLogin
          AND p.PermissionKey = N'AdminAttendanceDashboard'
    )
    BEGIN
        THROW 50004, 'Postcondition failed: AuthGuard-shaped HasPermission probe for AdminAttendanceDashboard returned no row.', 1;
    END;

    -- Every target key that exists in catalog must be granted to Super Admin
    IF EXISTS (
        SELECT 1
        FROM #TargetKeys t
        INNER JOIN dbo.Permissions p ON p.PermissionKey = t.PermissionKey
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.RolePermissions rp
            WHERE rp.RoleId = @RoleId
              AND rp.PermissionId = p.PermissionId
        )
    )
    BEGIN
        THROW 50005, 'Postcondition failed: one or more existing target keys still ungranted to Super Admin.', 1;
    END;

    -- Do not require keys that remain missing from catalog (except AAD, already asserted)
    COMMIT TRANSACTION;

    SELECT
        N'POST' AS SnapshotPhase,
        @AadPermissionId AS AdminAttendanceDashboardPermissionId,
        @InsertedGrants AS RolePermissionsRowsInsertedThisRun,
        (SELECT COUNT_BIG(*) FROM dbo.RolePermissions WHERE RoleId = @RoleId) AS SuperAdminRolePermissionCount,
        (SELECT COUNT_BIG(*)
         FROM #TargetKeys t
         INNER JOIN dbo.Permissions p ON p.PermissionKey = t.PermissionKey
         INNER JOIN dbo.RolePermissions rp ON rp.RoleId = @RoleId AND rp.PermissionId = p.PermissionId
        ) AS TargetKeysGranted,
        (SELECT COUNT_BIG(*)
         FROM #TargetKeys t
         WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.PermissionKey = t.PermissionKey)
        ) AS TargetKeysStillMissingFromCatalog;

    SELECT N'STILL_MISSING_CATALOG' AS Finding, t.PermissionKey
    FROM #TargetKeys t
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.PermissionKey = t.PermissionKey)
    ORDER BY t.PermissionKey;

    PRINT N'live_superadmin_nav_grant: COMMIT OK. AadPermissionId='
        + CONVERT(nvarchar(11), @AadPermissionId)
        + N'; grantsInserted='
        + CONVERT(nvarchar(11), @InsertedGrants)
        + N'. Unrelated RolePermissions preserved. Missing catalog keys listed above were not inserted.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-----------------------------------------------------------------------------
-- 7. Narrow rollback (manual; reverse only this script's AAD grant if needed)
--    Does NOT remove other Super Admin grants added for existing nav keys.
--    To reverse nav grants selectively, delete RolePermissions for keys that
--    were newly inserted (use PRE/POST snapshots). Never DELETE unrelated roles.
-----------------------------------------------------------------------------
/*
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'flamex_live' BEGIN RAISERROR('Rollback requires flamex_live.',16,1); RETURN; END;

BEGIN TRANSACTION;

-- Optional: remove only AdminAttendanceDashboard mapping for Super Admin RoleId 1
DELETE rp
FROM dbo.RolePermissions rp
INNER JOIN dbo.Permissions p ON p.PermissionId = rp.PermissionId
INNER JOIN dbo.Roles r ON r.RoleId = rp.RoleId
WHERE r.RoleId = 1
  AND r.RoleName = N'Super Admin'
  AND r.CompanyID = 1
  AND p.PermissionKey = N'AdminAttendanceDashboard';

-- Catalog row for AdminAttendanceDashboard is left in place (menu/SecurePage key).
-- DELETE FROM dbo.Permissions WHERE PermissionKey = N'AdminAttendanceDashboard';
-- only if zero RolePermissions reference it and product approves catalog removal.

COMMIT TRANSACTION;
*/
GO
