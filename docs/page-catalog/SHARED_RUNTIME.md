# Shared runtime (non-page)

Facts that belong to **handlers, user controls, helpers, SQL scripts, and stored-procedure call sites** — not to a `.aspx` census. Read after [`SHARED_CONTEXT.md`](SHARED_CONTEXT.md). Live objects vs missing UAT tables: [`SHARED_SCHEMA.md`](SHARED_SCHEMA.md). Do **not** copy AuthN, tenancy, or Bill.Master here.

---

## 1. HTTP handlers (`.ashx`)

| Handler | Auth | Unique job |
|---------|------|------------|
| `corporate/business/app/Heartbeat.ashx` | Session `SessionToken` (GUID) | JSON `{ status: ok \| logout, reason? }`. Reads/updates `dbo.ActiveSessions` (`IsActive`, `LastHeartbeat`). Idle **30 minutes**. `IsReusable = false`. Connection: `DbConn`. |
| `corporate/business/app/clientHandlerAdmin.ashx` | **None** | Typeahead: `SELECT Client_Name FROM tbl_Client WHERE Client_Name LIKE @term + '%'`. **No `CompanyID`**. Sets `CommandType.StoredProcedure` on a text `SELECT` (leftover bug). Query `term`. |
| `admin/personal_image.ashx` | **None** | Streams `tbl_employee.imgdata` as `image/jpeg`. QS **`ID`**. Used by kiosk print `Print/id_card1.aspx` and `admin/Total_id_card.aspx`. |
| `admin/Company.ashx` | **None** | Streams `tbl_Company.Signe` as `image/jpeg`. QS **`ComID`**. Kiosk company mark, not ERP `CompanyContext`. |

There is **no** `card/card_image.ashx`. Kiosk photos go through `personal_image.ashx`.

---

## 2. User controls (`.ascx`)

The only `.ascx` in the tree is `corporate/business/app/GlobalNotification.ascx`, hosted on `Bill.Master`.

| Fact | Detail |
|------|--------|
| Intended job | `sp_GetActiveNotifications` `@UserId` → repeater; dismiss → `sp_MarkNotificationRead`. Hide when no `Session["USERID"]`. |
| Leftover | `OnInit` **throws** `GlobalNotification code-behind executed`. If the compiled control runs, every Bill.Master page faults. Distinct from `tbl_SystemNotification` INSERT helpers used on many pages. |

Ponytail CSS/JS is not a user control here — see [`SHARED_CONTEXT.md`](SHARED_CONTEXT.md). Card kiosk uses `card.Master`, not this ASCX.

---

## 3. Helper / service classes (unique jobs)

| Type | Unique job |
|------|------------|
| `AuthGuard` | Cookie + session + `ActiveSessions`; `EnsurePage` / `EnsurePrint` / `HasPermission`. Contract: [`docs/22_Security_Baseline.md`](../22_Security_Baseline.md). |
| `SecurePage` | Base page; `OnInit` → `AuthGuard.EnsurePage` using `RequiredPermissionKey` / `RequiredAnyPermissionKeys`. |
| `CompanyContext` | Nested on `Bill.Master.cs`. `CurrentCompanyID` from `Session["CompanyID"]`. |
| `AppSecrets` | `DbConn` connection string; `GetAppSetting` / `GetInt`; `GetUrlTokenAesKey` (empty key keeps legacy QuickAction decrypt). |
| `DB_UTILITY` | Legacy ADO wrapper (`Sqlconnection` / `ConnectDb` / `SPreturn_dt` is often **not** an SP). Many pages still `new SqlConnection`. |
| `PasswordHasher` | PBKDF2 `Rfc2898DeriveBytes`, 100k iterations, 16-byte salt, 32-byte hash; SHA-256 hex for reset tokens; legacy plaintext upgrade path. |
| `PasswordResetService` | Table **`dbo.PasswordResetTokens`** (not `tbl_PasswordResets`). 32-byte hex token, SHA-256 store, default 30 min (`PasswordResetTokenMinutes`). SQL 208 (missing table) fails closed. Completes via `reset_password.aspx`. **UAT does not have this table** ([SHARED_SCHEMA.md](SHARED_SCHEMA.md) §4). |
| `SecurityHelper` | AES URL token (`EncryptToUrlToken` / `DecryptFromUrlToken`) for **`QuickAction.aspx?t=`**. Not HMAC. Key from `AppSecrets.GetUrlTokenAesKey`. |
| `CommunicationGateway` | Config SMTP (`SmtpFrom`/`SmtpUser`/`SmtpPass`/`SmtpHost`/`SmtpPort`/`SmtpEnableSsl`) + MSG91 `Sendhttp`. `SendCustomEmail` / `SendAlertsAsync` (email and/or WhatsApp). Fail-open (swallow). Visit/HR/index use this. |
| `InvoiceListHelper` | Shared invoice list SQL/format + **ClosedXML `.xlsx`** export (`ExportVersion` v3). Used by `View_Invoice` / `seartch_invoice`. |
| `PurchaseOrderPrintHelper` | HTML bind for **client PO** print (`NewPurchaseOrder_Print.aspx?ID=` = `tbl_Quotation.ID`). Not vendor `tbl_PO_Header`. |
| `MoneyConvDS` | Amount-in-words on print pages. |
| `CryptoRandom` | Token/salt bytes for hasher and reset. |
| `App_Start/AuthConfig` | Empty OpenAuth stub (`RegisterOpenAuth`). Not ERP login. |
| `App_Start/BundleConfig` | Script/style bundles. Not AuthN. |

Do **not** treat `DB_UTILITY` as the only data path.

### Bill.Master support ticket (not a helper class)

`CreateiTopTicket` POSTs to `iTopUrl` with `iTopUser` / `iTopPass` / `iTopCallerEmail` / `iTopOrgName` (screenshot base64). `SendSupportEmail` is a **direct `SmtpClient`** to a hardcoded IT inbox using AppSettings SMTP — same leftover class as InvoiceMail, not `CommunicationGateway`.

---

## 4. SQL scripts in this repo (not live DDL)

The **live database is not versioned**. These files are patches or snapshots. Do **not** paste bodies into domain catalogs. What actually exists on **`flamex_uat`**: [`SHARED_SCHEMA.md`](SHARED_SCHEMA.md). `UserCompanyAccess.sql` and `PasswordResetTokens.sql` are **not applied** on that copy.

| Path | Unique contents |
|------|-----------------|
| [`db/pservice_snapshot.sql`](../../db/pservice_snapshot.sql) | `pservice` dump used by CI. |
| [`docs/invoice_pservice_backfill.sql`](../invoice_pservice_backfill.sql) | Invoice primary-service backfill. |
| [`docs/qutprimaryservice_companyid_backfill.sql`](../qutprimaryservice_companyid_backfill.sql) | Quotation primary-service `CompanyID` backfill. |
| `Bill_Software/corporate/business/sql/PasswordResetTokens.sql` | `dbo.PasswordResetTokens` DDL. |
| `…/sql/CompanyID_Roles_ActiveSessions.sql` | Tenant / roles / sessions objects. |
| `…/sql/UserCompanyAccess.sql` | `UserCompanyAccess` membership. |
| `…/sql/user_company_access_reconciliation.sql` | Membership repair. |
| `…/sql/user_roles_reconciliation.sql` | `UserRoles` repair. |
| `…/sql/ensure_purchaseorder_schema.sql` | Vendor PO objects. |
| `…/sql/requisition_po_companyid.sql` | PR/PO `CompanyID`. |
| `…/sql/tbl_NewProduct_CompanyID.sql` | Product tenant column. |
| `…/sql/tbl_NewparentProduct_CompanyID.sql` | Parent-product tenant column. |
| `…/sql/tbl_Attendance_GeoFenceOverride.sql` | Attendance geofence override. |
| `…/sql/tbl_login_GeoFenceFallback.sql` | Login geofence fallback. |

Visit workflow has **no** `.sql` DDL in-repo. Table names are reverse-engineered in [`sales-visit-workflow-audit/02_Database_Dependency_Map.md`](../sales-visit-workflow-audit/02_Database_Dependency_Map.md). Live visit table is **`tbl_SalesVisitReport`**, not a table named `daily_rpt`.

---

## 5. Stored procedures called from C# (names + callers)

Bodies live in SQL Server. This is a **call-site index** only. `clientHandlerAdmin` is omitted (it is not a real SP).

| Procedure | Callers | Unique purpose |
|-----------|---------|----------------|
| `sp_RunAttendanceRulesEngine` | `attendance.aspx.cs`, `AdminShiftAssignment.aspx.cs` | Recalc attendance after punch / shift map. |
| `sp_AllocateEmployeeLeaves` | `AddUser.aspx.cs` | Initial leave balances on new ERP user. |
| `sp_GetActiveNotifications` | `GlobalNotification.ascx.cs` | Toast feed (control currently throws in `OnInit`). |
| `sp_MarkNotificationRead` | `GlobalNotification.ascx.cs` | Dismiss one notification. |
| `sp_Requisition_CreateDraft` | `RequisitionNew.aspx.cs` | Modern PR draft header. |
| `sp_SubmitRequisition` | `RequisitionNew.aspx.cs`, `View_PR_Details.aspx.cs` | Submit PR. |
| `sp_RequisitionItem_BulkUpsert` | `RequisitionNew.aspx.cs`, `View_PR_Details.aspx.cs` | Line upsert (TVP). |
| `sp_CancelRequisition` | `RequisitionNew.aspx.cs`, `View_PR_Details.aspx.cs` | Cancel PR. |
| `sp_Requisition_Approve` | `Approve_PR.aspx.cs`, `View_PR_Details.aspx.cs` | Approve submitted PR. |
| `sp_GeneratePO_FromReqNo` | `Generate_PO_Preview.aspx.cs` | Vendor PO from approved PR. |
| `sp_ReleasePO_Final` | `View_PO_Details.aspx.cs` | Final-release vendor PO. |
| `sp_GetReleasedPO_Details` | `print/Print_PO.aspx.cs` | Vendor PO print payload. |
| `sp_getapplock` | `Create_quotation.aspx.cs`, `Edit_quatation_v2.aspx.cs`, `edit_purchaseorder.aspx.cs` | SQL applock around quote/client-PO save. |
| `sp_SearchProductsFast` | `Product_stock.aspx.cs` | Stock page typeahead. |
| `sp_GetProductStockByStore` | `Product_stock.aspx.cs` | Stock by store. |
| `sp_GetProductCategories` | `Product_stock.aspx.cs` | Category list. |

Visit/report SQL is **inline** on `tbl_SalesVisitReport` / `tbl_Expenses` / `tbl_SalesVisitResponses`.

Full reverse map (every table/SP → pages with verbs): [`DATA_DICTIONARY.md`](DATA_DICTIONARY.md).
Every UAT procedure **signature** (parameters, not body): [`UAT_CATALOG.md`](UAT_CATALOG.md).

UAT also has procs **not** called from C# (amendment/cancel family, `sp_ApproveRequisition` vs `sp_Requisition_Approve`, `sp_ReleasePO` vs `sp_ReleasePO_Final`, stock `usp_*`, `InsertOrGetProduct`, …). Inventory: [`SHARED_SCHEMA.md`](SHARED_SCHEMA.md) §10. Do not paste bodies.

---

## 6. Web.config keys (names only)

Connection: **`DbConn`** (not `DBCS`). Do not paste secret values.

| Key | Consumer |
|-----|----------|
| `SmtpHost` / `SmtpPort` / `SmtpUser` / `SmtpPass` / `SmtpFrom` / `SmtpEnableSsl` | `CommunicationGateway`; some pages still construct `SmtpClient` themselves (`InvoiceMail`, `ProformaMail`, `PaymentMail`, `Set_quatation`, `ViewUser`, `Update/password`) |
| `PasswordResetTokenMinutes` | `PasswordResetService` (default 30) |
| `LoginOtpExpiryMinutes` | Login OTP |
| `UrlTokenAesKey` | `SecurityHelper` / QuickAction |
| `Msg91AuthKey` / `Msg91IntegratedNumber` / `Msg91OtpTemplateId` | WhatsApp/SMS via gateway / OTP |
| `iTopUrl` / `iTopUser` / `iTopPass` / `iTopCallerEmail` / `iTopOrgName` | `Bill.Master.CreateiTopTicket` (not SMTP) |

There is **no** `AppUrl` or `Requisition:AttachmentsPath` key in the current `Web.config`.
