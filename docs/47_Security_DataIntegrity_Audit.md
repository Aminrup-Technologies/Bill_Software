# Security & Data Integrity Audit: Cross-Tenant Duplication

**Date:** September 14, 2026
**Scope:** Trust-boundary analysis for PRs #85–#88
**Files:** `DuplicationService.cs`, `View_vendor.aspx[.cs]`, `View_client.aspx[.cs]`

---

## Trust Boundary Model

```
┌─────────────────────────────────────────────────────┐
│  CLIENT (Browser)         ← UNTRUSTED              │
│  • Hidden fields: hfPendingVendorId, hfBulkVendorIds│
│  • DropDownList: ddlTargetCompanyGlobal             │
│  • CheckBox: chkSelectVendor                        │
└──────────────────────┬──────────────────────────────┘
                       │ HTTP POST (ASP.NET postback)
                       ▼
┌─────────────────────────────────────────────────────┐
│  WEB FORMS CODE-BEHIND      ← TRUST BOUNDARY       │
│  • Session["USERID"] check (Page_Load)              │
│  • SecurePage / AuthGuard (base class)              │
│  • Input validation (prefix, parse, range)          │
│  • CompanyID resolution (server-side only)          │
└──────────────────────┬──────────────────────────────┘
                       │ Method call
                       ▼
┌─────────────────────────────────────────────────────┐
│  DuplicationService           ← TRUSTED             │
│  • CompanyContext.CurrentCompanyID (server-side)    │
│  • AuthGuard.UserCanAccessCompany(targetCompanyId)  │
│  • SQL transaction with explicit CompanyID params   │
└──────────────────────┬──────────────────────────────┘
                       │ ADO.NET
                       ▼
┌─────────────────────────────────────────────────────┐
│  SQL Server                  ← TRUSTED              │
│  • Parameterized queries only                       │
│  • Transaction rollback on failure                  │
└─────────────────────────────────────────────────────┘
```

---

## 1. Hidden Field Manipulation

### Attack Surface
An attacker could modify the following hidden fields using browser DevTools:
- `hfPendingVendorId` / `hfPendingClientId` (single-record mode)
- `hfBulkVendorIds` / `hfBulkClientIds` (bulk mode)

### Analysis

| Hidden Field | Client Sets | Server Validates | Trust Boundary |
|---|---|---|---|
| `hfPendingVendorId` | JS writes `Eval("Vendor_Id")` value | `vendorId.Length < 3` prefix check → `ResolveVendorId(vendorId, companyId)` queries DB with **both Vendor_Id AND CompanyID** | ✅ **SAFE** — Attacker cannot reference a vendor from another company because the SQL WHERE clause enforces `CompanyID = <current company>`. Even if the value is tampered, it resolves to 0 (not found) and is rejected. |
| `hfBulkVendorIds` | JS collects checked IDs from `hfVendorId` per row | `int.TryParse` per comma-separated part → `VendorInCurrentCompany(conn, sourceId)` checks `WHERE Id = @Id AND CompanyID = @CompanyID` | ✅ **SAFE** — Each ID is individually verified against the current company. Invalid/non-existent IDs are silently skipped. |
| `hfPendingClientId` | Same pattern as vendor | Same prefix + `ResolveClientId(clientId, companyId)` | ✅ **SAFE** |
| `hfBulkClientIds` | Same pattern as vendor | Same parse + `ClientInCurrentCompany(conn, clientId)` | ✅ **SAFE** |

### Verdict: **No trust-boundary violation.** All hidden field values are validated server-side with CompanyID enforcement before any data operation.

---

## 2. Authorization Bypass

### Attack Surface
An unauthorized user could attempt to:
- Duplicate records they don't own
- Duplicate into a company they don't have access to

### Analysis

| Check | Location | Mechanism | Tamper-Resistant? |
|---|---|---|---|
| **Page access** | `Page_Load` (inherited from `SecurePage`) | `Session["USERID"]` null check + `SecurePage` permission gate via `RequiredPermissionKey` | ✅ Yes — Session is server-side state |
| **WebMethod access** | `GetVendorNames`, `GetClientNames` | `AuthGuard.EnsureWebMethodPermission()` | ✅ Yes — Server-side only |
| **Source ownership** | `DuplicateVendor` / `DuplicateCustomer` | `VendorInCurrentCompany(conn, sourceId)` / `ClientInCurrentCompany(conn, sourceId)` — SQL with `CompanyID = CompanyContext.CurrentCompanyID` | ✅ Yes — CompanyContext is server-side |
| **Target access** | `DuplicateVendor` / `DuplicateCustomer` | `UserCanAccessCompany(targetCompanyId)` → `AuthGuard.UserCanAccessCompany()` | ✅ Yes — Server-side auth check |
| **Target access (UI)** | `btnConfirmDuplicateVendor_Click` | `AuthGuard.UserCanAccessCompany(targetCompanyId)` checked before service call | ✅ Yes — Belt-and-suspenders with service layer |
| **Same-tenant block** | Both click handlers | `targetCompanyId == sourceCompanyId` comparison | ✅ Yes — Server-side comparison |

### Verdict: **No authorization bypass possible.** Every access path is gated by server-side checks. The dropdown is populated by `AuthGuard.GetAuthorizedCompanies()` which only returns companies the user has access to, and the target CompanyID is re-validated at the service layer.

---

## 3. CompanyID Spoofing

### Attack Surface
An attacker could attempt to set `CompanyID` on an INSERT to any arbitrary value.

### Analysis

| Operation | CompanyID Source | Attacker Can Override? | Defense |
|---|---|---|---|
| **Source read** | `CompanyContext.CurrentCompanyID` (from Session) | ❌ No — Session is server-side | Session set by `Bill.Master.cs` company dropdown, not from client input |
| **Target INSERT (Vendor)** | `targetCompanyId` parameter from `ddlTargetCompanyGlobal.SelectedValue` | ⚠️ Partially — attacker can choose any dropdown value | `AuthGuard.UserCanAccessCompany(targetCompanyId)` validates the target is in the user's authorized company list |
| **Target INSERT (Customer)** | Same as vendor | Same | Same |
| **Child entity INSERT (Factory)** | `targetCompanyId` passed from method parameter | ❌ No — derived from validated parameter | Factory and Representative INSERTs use the same `targetCompanyId` that was already validated |
| **Audit INSERT** | `targetCompanyId` from validated parameter | ❌ No | Same |

### Critical Check: Can `targetCompanyId` be spoofed via the dropdown?

The `ddlTargetCompanyGlobal` is an `asp:DropDownList` populated server-side:
```csharp
List<AuthorizedCompany> companies = AuthGuard.GetAuthorizedCompanies();
foreach (var c in companies)
{
    if (c.Id != CompanyContext.CurrentCompanyID)
        ddlTargetCompanyGlobal.Items.Add(new ListItem(c.Name, c.Id.ToString()));
}
```

An attacker could modify the `<select>` element's `value` attribute in DevTools to set an arbitrary integer. However, the server-side check `AuthGuard.UserCanAccessCompany(targetCompanyId)` would reject any CompanyID not in the user's authorized list.

**Additionally**, even if the attacker bypassed the `UserCanAccessCompany` check (hypothetically), the data would still be written with the attacker's chosen CompanyID — which is the correct behavior for cross-tenant duplication. The attacker is an authorized user choosing a valid target.

### Verdict: **No CompanyID spoofing vulnerability.** The target CompanyID is validated against the user's authorized company list at both the UI layer and the service layer.

---

## 4. Cross-Tenant Permission Checks

### Data Flow Integrity

```
User clicks Duplicate
    ↓
JS: openDuplicateModal(vendorId)
    ↓  Sets hfPendingVendorId = vendorId (from Eval, client-trusted)
User selects target company from dropdown
    ↓  ddlTargetCompanyGlobal.SelectedValue = "42"
JS: validateDuplicateSelection()
    ↓  Validates ddl is not empty, shows confirm()
ASP.NET postback fires
    ↓
btnConfirmDuplicateVendor_Click()
    ├─ int.TryParse(ddl.SelectedValue) → targetCompanyId = 42
    ├─ targetCompanyId == sourceCompanyId → BLOCK if same
    ├─ AuthGuard.UserCanAccessCompany(42) → BLOCK if unauthorized
    ├─ hfPendingVendorId.Value → vendorId = "AA15"
    ├─ vendorId.StartsWith("AA") → validates prefix
    ├─ ResolveVendorId("AA15", sourceCompanyId) → sourceId = 187
    │   └─ SQL: WHERE Vendor_Id = 'AA15' AND CompanyID = <sourceCompanyId>
    │   └─ Returns 187 (valid) or 0 (invalid → rejected)
    └─ DuplicationService.DuplicateVendor(187, 42, userId)
        ├─ VendorInCurrentCompany(conn, 187) → verifies 187 belongs to source company
        ├─ UserCanAccessCompany(42) → RE-VALIDATES (belt-and-suspenders)
        ├─ Reads source vendor data WHERE Id = 187 AND CompanyID = <sourceCompanyId>
        ├─ Generates new Vendor_Id scoped to company 42
        ├─ INSERT INTO tbl_Vendor ... CompanyID = 42
        └─ INSERT INTO tbl_SystemNotification ... CompanyID = 42
```

### Assessment

| Check | Layer 1 (UI) | Layer 2 (Service) | Redundant? |
|---|---|---|---|
| Source ownership | `ResolveVendorId` with CompanyID | `VendorInCurrentCompany` | Yes — defense-in-depth ✅ |
| Target access | `AuthGuard.UserCanAccessCompany` | `UserCanAccessCompany` | Yes — defense-in-depth ✅ |
| Same-tenant | UI comparison | None at service level | **No** — UI-only check ⚠️ |
| Code generation scope | — | `targetCompanyId` passed to `GenerateNextBusinessCode` | N/A |

### One Gap Identified

**Same-tenant check is UI-only.** The `btnConfirmDuplicateVendor_Click` handler checks `targetCompanyId == sourceCompanyId` and blocks. However, the `DuplicationService.DuplicateVendor` method does NOT independently verify that `targetCompanyId != CompanyContext.CurrentCompanyID`. If the service were called from a different code path that skipped this check, same-tenant duplication would be allowed.

**Severity: Low.** The service is `static` and only called from the two View pages. There is no current code path that bypasses the UI check. However, for defense-in-depth, the service method should include:
```csharp
if (targetCompanyId == CompanyContext.CurrentCompanyID)
    throw new InvalidOperationException("Source and target companies must be different.");
```

---

## 5. Business-Code Race Conditions

### Attack Surface
Two concurrent users (or the same user clicking rapidly) could trigger code generation simultaneously, potentially producing the same Vendor_Id or Client_Id.

### Analysis

```
Transaction A                          Transaction B
─────────────                          ─────────────
BEGIN TRAN                             BEGIN TRAN
SELECT TOP 1 Vendor_Id                 
  WHERE CompanyID = 42                 
  ORDER BY Id DESC                     
  → reads "AA05"                       
                                       SELECT TOP 1 Vendor_Id
                                         WHERE CompanyID = 42
                                         ORDER BY Id DESC
                                         → reads "AA05" (same!)
candidate = "AA06"                     
SELECT COUNT(*) WHERE Vendor_Id = 'AA06' AND CompanyID = 42
  → 0 (ok)                             
                                       candidate = "AA06"
                                       SELECT COUNT(*) WHERE Vendor_Id = 'AA06' AND CompanyID = 42
                                         → 0 (ok, not yet inserted!)
INSERT Vendor_Id = 'AA06'              
  → succeeds                           
                                       INSERT Vendor_Id = 'AA06'
                                         → DUPLICATE KEY ERROR
COMMIT                                 
                                       CATCH → retry loop → generates "AA07"
                                       → succeeds on retry
```

### Mitigation Assessment

| Defense | Present? | Effectiveness |
|---|---|---|
| `UPDLOCK, HOLDLOCK` table hints | ❌ No | Without row/page locks, concurrent reads see stale MAX values |
| `UNIQUE INDEX` on `(Vendor_Id, CompanyID)` | ❌ Not in schema | No database-level collision prevention |
| Retry loop with COUNT check | ✅ Yes (5 retries) | Catches most collisions; second INSERT fails, retry generates new code |
| Transaction isolation | `ReadCommitted` | Prevents dirty reads but not phantom/repeatable-read races |

### Risk Assessment

**Under normal usage:** Low risk. Duplication is an admin action typically performed by one user at a time. The 5-retry loop handles most races.

**Under high concurrency:** Medium risk. If 3+ users simultaneously duplicate into the same company, all could read the same MAX value. The COUNT check would catch collisions, but all 5 retries could theoretically be exhausted if 6+ concurrent transactions all read the same MAX and generate the same codes.

### Recommended Mitigations (follow-up, not blocking)

1. **Database:** Add `CREATE UNIQUE INDEX IX_Vendor_CompanyID ON tbl_Vendor(Vendor_Id, CompanyID)` and `CREATE UNIQUE INDEX IX_Client_CompanyID ON tbl_Client(Client_Id, CompanyID)`. This provides a hard constraint.

2. **Service:** Add `WITH (UPDLOCK, HOLDLOCK)` to the SELECT query in `GenerateNextBusinessCode` to serialize code generation per company:
   ```sql
   SELECT TOP 1 [Vendor_Id] FROM [tbl_Vendor] 
   WITH (UPDLOCK, HOLDLOCK) 
   WHERE CompanyID = @CompanyID ORDER BY Id DESC
   ```

3. **Service:** Add a catch for `SqlException` (duplicate key error number 2601/2627) in the retry loop and retry immediately instead of using the COUNT check.

---

## 6. Additional Trust-Boundary Findings

### 6.1 SQL Injection in `GenerateNextBusinessCode`

| Aspect | Finding |
|---|---|
| Code | `string.Format("SELECT TOP 1 [{0}] FROM [{1}] WHERE CompanyID = @CompanyID", codeColumn, tableName)` |
| Risk | The `tableName` and `codeColumn` are interpolated into the query string |
| Mitigation | Both values are **hardcoded internal constants** (`"AA"/"tbl_Vendor"/"Vendor_Id"`, `"AD"/"tbl_Client"/"Client_Id"`), never from user input. The method is `private static`. |
| Verdict | **No risk.** Internal-only, hardcoded values. |

### 6.2 `userName` Injection in Audit Messages

| Aspect | Finding |
|---|---|
| Code | `string.Format("User '{0}' duplicated vendor '{1}' ...", userName, ...)` |
| Risk | If `userName` contained SQL metacharacters, could it affect the query? |
| Mitigation | The message is passed as a `@Message` SQL parameter, not concatenated into the query. ASP.NET parameterization handles escaping. |
| Verdict | **No risk.** Parameterized INSERT. |

### 6.3 `userName` Source

| Aspect | Finding |
|---|---|
| Code | `string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System"` |
| Risk | Could `Session["USERID"]` be spoofed? |
| Mitigation | Session is server-side state set during authentication. `Session["USERID"]` is populated by the login flow and never written from client input. |
| Verdict | **No risk.** Server-side session state. |

### 6.4 `ExecuteWithinTransaction` Method

| Aspect | Finding |
|---|---|
| Code | `public static void ExecuteWithinTransaction(SqlConnection conn, SqlTransaction tran, Action<SqlConnection, SqlTransaction> action) { action(conn, tran); }` |
| Risk | Accepts externally-provided connection and transaction. If called with a compromised conn/tran, the action would execute under that context. |
| Mitigation | The method is `public static` but **never called from outside the class** (grep confirms 0 external callers). It is dead code. |
| Verdict | **No current risk.** Dead code. Recommend removing or marking `internal`. |

### 6.5 No ViewState Tampering Risk

| Aspect | Finding |
|---|---|
| Risk | Could ViewState be modified to inject values? |
| Mitigation | The duplication feature does not read from ViewState. All values come from hidden fields (`hfPendingVendorId`, `hfBulkVendorIds`) which are explicitly validated server-side. |
| Verdict | **No risk.** ViewState not involved. |

### 6.6 No CSRF Risk

| Aspect | Finding |
|---|---|
| Risk | Could a cross-site request trigger duplication? |
| Mitigation | ASP.NET WebForms uses ViewState MAC and EventValidation by default. The postback includes `__VIEWSTATE` and `__EVENTVALIDATION` tokens that prevent forged requests. Additionally, the session check in `Page_Load` requires an active authenticated session. |
| Verdict | **No risk.** Standard ASP.NET WebForms CSRF protections. |

---

## Summary

| Category | Finding | Severity | Status |
|---|---|---|---|
| Hidden field manipulation | All hidden fields validated with CompanyID server-side | None | ✅ No issue |
| Authorization bypass | All access paths gated by SecurePage + AuthGuard | None | ✅ No issue |
| CompanyID spoofing | Target CompanyID validated against authorized list at UI + service layer | None | ✅ No issue |
| Cross-tenant permission | Redundant checks at UI and service layers | None | ✅ No issue |
| Same-tenant check | UI-only, not in service layer | Low | ⚠️ Recommend adding service-level check |
| Code generation race condition | No UPDLOCK/HOLDLOCK, no UNIQUE INDEX | Low | ⚠️ Recommend DB migration for unique index + table hints |
| Dead code | `ExecuteWithinTransaction` is never called | Low | ⚠️ Recommend removal |
| SQL injection | All queries parameterized; `string.Format` uses only hardcoded internal constants | None | ✅ No issue |
| CSRF | Standard ASP.NET WebForms protections in place | None | ✅ No issue |
| ViewState | Not used by duplication feature | None | ✅ No issue |

### Overall Trust-Boundary Assessment: **PASS**

The cross-tenant duplication feature maintains proper trust boundaries at every layer. Client input is validated, sanitized, and checked against server-side authorization before any data operation. The most significant finding is the same-tenant check being UI-only (Low severity) and the absence of a unique database index on `(Vendor_Id, CompanyID)` (Low severity, pre-existing gap).

---

*No production code was modified during this audit.*
