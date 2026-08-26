# 04 — Security & Tenant Audit

> This document consolidates: **(E)** tenancy/authorization filter audit, **(G)** file/attachment handling & retrieval security, **(J)** edit-locking rules, and **(K)** email trigger/recipient/SMTP-configuration audit. Per instructions, **no live credentials are reproduced** — any secret found in source is redacted as `[REDACTED]` with only its *location* cited.

---

## E. Security / Tenancy Filter Audit

### E.1 Inventory of every query, grouped by which identity filter (if any) it applies

| File :: Method | Query Purpose | `CompanyID` | `CreatedByCode` | `USERID` (session) | `ReportingManagerId` | Notes |
|---|---|:---:|:---:|:---:|:---:|---|
| `visit_planner.aspx.cs :: GetCalendarEvents` | List current user's calendar events | ❌ | ✅ (`= @UserId` from Session) | ✅ (source of `@UserId`) | ❌ | Self-service; correct scope for *this* user, but never validated against `CompanyID` |
| `visit_planner.aspx.cs :: GetVisitDetails` | Fetch one visit's full detail for the "view" modal | ❌ | ❌ | ❌ | ❌ | **No ownership check whatsoever** — `WHERE Id=@Id` only. Any authenticated user who can guess/enumerate an `Id` can retrieve another user's (or another company's) visit detail via this PageMethod. |
| `visit_planner.aspx.cs :: btnSubmitExecution_Click` | Execute a visit (UPDATE) + spawn follow-up (INSERT) | ❌ | ❌ | ❌ | ❌ | `UPDATE ... WHERE Id=@Id` — **no `CreatedByCode` or `CompanyID` predicate.** Any logged-in user who can drive a postback with an arbitrary `hfExecuteVisitId` value can execute (and GPS-tag, attach a file to, and change the status of) **any other user's** Planned visit. |
| `daily_rpt.aspx.cs :: GetAdminName` | Look up display name for the logged-in user | ❌ | n/a | ✅ (used directly, string-concatenated) | ❌ | Read-only, self-referential; low risk despite raw concatenation (input is the session's own `USERID`, not user-supplied) |
| `daily_rpt.aspx.cs :: btnSubmit_Click` | Create a new visit | ❌ (never set) | ✅ (written as `CreatedByCode`) | ✅ (source), with **hardcoded fallback `"FLM03"`** if session value is null | ❌ | See D-01/D-03 in `05_Potential_Defects.md` |
| `vw_dailyrpts.aspx.cs :: BindSalesVisits` | List current user's own visits | ❌ | ✅ (`= @CreatedByCode` from Session) | ✅ | ❌ | Self-service list is correctly scoped to the owner, but not to `CompanyID` (irrelevant here since it's already scoped to one user, but inconsistent with the pattern used elsewhere) |
| `vw_dailyrpts.aspx.cs :: LoadMegaModal` | Load one visit's full file (detail + expenses + chat) for view/edit | ❌ | ❌ | ❌ | ❌ | `WHERE v.Id=@Id` only. The GridView row only ever exposes IDs belonging to the current user in normal use, but the server method itself does not re-verify `CreatedByCode` — a forged `CommandArgument`/`hfMegaVisitId` postback could load **any** visit's full file, including cross-tenant. |
| `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click` | Update visit details | ❌ | ❌ | ❌ | ❌ | `UPDATE ... WHERE Id=@Id AND ApprovalStatus='Pending' AND NOT EXISTS(manager comment)` — enforces the *edit-lock* business rule but **not ownership**. Any user can edit any other user's still-`Pending`, not-yet-commented visit if they can supply its `Id`. |
| `vw_dailyrpts.aspx.cs :: btnMegaSendChat_Click` | Post a chat message | ❌ | ❌ (implicit, hardcoded role='Salesperson') | ✅ (used as `RespondentCode`) | ❌ | Inserts under the *current* user's own code, but does not verify the current user is actually the visit's `CreatedByCode` before allowing a "Salesperson" reply on someone else's visit thread. |
| `srch_dailyrpts.aspx.cs :: Page_Load` (dropdown) | List salespeople for filter dropdown | ✅ (`= @CompanyID`) | n/a | n/a | ❌ | Correctly tenant-scoped |
| `srch_dailyrpts.aspx.cs :: Binder` | Manager's search across all visits | ✅ (`= <int>`, **string-concatenated**) | optional (string-concatenated) | n/a | ❌ | Tenant-scoped, but **not parameterized** — see D-05 (SQL injection) in `05_Potential_Defects.md` |
| `srch_dailyrpts.aspx.cs :: LoadMegaModal` | Load one visit's full file for the manager view | ❌ | ❌ | ❌ | ❌ | `WHERE Id=@Id` only — **no `CompanyID` re-check**, even though the *list* that produced this `Id` was `CompanyID`-scoped. A tampered `hfMegaVisitId`/`CommandArgument` value from a **different company** would still load successfully. |
| `srch_dailyrpts.aspx.cs :: ProcessApproval` | Approve/Reject a visit | ❌ | ❌ | ✅ (used as `ApprovedBy`) | ❌ | `UPDATE ... WHERE Id=@Id` only — no `CompanyID` check, and critically **no check that the acting user is the visit creator's `ReportingManagerId`.** Any user who can reach this page (any employee whose account can load `srch_dailyrpts.aspx`) can approve/reject **any** visit in **any** company, limited only by whether they can produce a valid `Id`. |
| `srch_dailyrpts.aspx.cs :: gvMegaExpenses_RowCommand` (Approve/Reject expense) | Approve/Reject one expense | ❌ | ❌ | ✅ (used as `ApprovedBy`) | ❌ | Same gap as visit approval, replicated for expenses |
| `srch_dailyrpts.aspx.cs :: GetUserRole` | Infer chat sender's role | n/a | ✅ (equality test against visit's `CreatedByCode`) | n/a | ❌ | If the sender is *not* the visit's creator, the method **defaults to `"Manager"`** regardless of who they actually are — there is no check that they are the *correct* manager (via `ReportingManagerId`) or even that they belong to the same company. |
| `vw_dailyrpts.aspx.cs` / `srch_dailyrpts.aspx.cs :: SendChatEmailNotification` / `SendApprovalNotification` | Resolve recipient e-mail | n/a | n/a (via join) | n/a | ✅ (`LEFT JOIN tbl_login Manager ON Manager.User_Id = Creator.ReportingManagerId`) | **The only place `ReportingManagerId` is used at all** — purely for e-mail routing, never for authorization |

### E.2 The Central Inconsistency

Two philosophies coexist in the same feature area without reconciliation:

1. **Self-service pages** (`visit_planner.aspx`, `vw_dailyrpts.aspx`) scope list queries by `CreatedByCode` (= the logged-in `USERID`) and **never reference `CompanyID` at all.**
2. **Manager/oversight pages** (`srch_dailyrpts.aspx`) scope their *list* query by `CompanyID`, matching the pattern already established elsewhere in the codebase (`Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs` — both of which carry an explicit code comment: `// Full-Stack CompanyContext segregation fix: Ensure Visit Report matches Company`, indicating a **deliberate remediation effort that was applied to those two files but never back-ported to the 8 files in this workflow's scope**).

Neither philosophy is applied to the **detail/mutation** endpoints (`GetVisitDetails`, both `LoadMegaModal` implementations, `btnSubmitExecution_Click`, `btnUpdateVisit_Click`, `ProcessApproval`, expense approve/reject) — every one of these operates on a bare `Id` with no ownership or tenant re-validation. This is the single most consequential architectural inconsistency identified in this audit (elaborated with severity/confidence ratings in `05_Potential_Defects.md`).

### E.3 Session / CSRF Posture

- The only authentication gate on all four in-scope pages (plus `expense_entry.aspx`) is `if (HttpContext.Current.Session["USERID"] == null) Response.Redirect("~/index.aspx");` in `Page_Load`. There is no secondary authorization check (role, company, manager relationship) beyond this single "are you logged in at all" gate.
- The two `[WebMethod]` PageMethods (`GetCalendarEvents`, `GetVisitDetails`) are marked `EnableSession = true` and are invoked via `fetch(..., { method: 'POST' })` from client script. No anti-forgery token, custom header check, or `SameSite` cookie enforcement was found protecting these endpoints or the standard WebForms postback handlers — the workflow relies entirely on ASP.NET's default session-cookie behavior for both authentication and (implicitly, weakly) CSRF resistance.

---

## G. Files / Attachments — Upload Path, Filename Generation, Retrieval, Security Implications

### G.1 Visit attachments (photo/document evidence)

| Aspect | Behavior | Source |
|---|---|---|
| Upload trigger | `fileExecAttachment` (visit_planner.aspx execute modal), `fileAttachment` (daily_rpt.aspx, mode=past), `edit_fileAttachment` (vw_dailyrpts.aspx edit form) | 3 independent, near-duplicate implementations |
| Storage path | `Server.MapPath("~/Uploads/")` (created via `Directory.CreateDirectory` if missing) | all three upload sites |
| Filename generation | `DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(<uploaded file name>)` | identical pattern in all three sites |
| Extension / content-type validation | **None.** No allow-list/deny-list of extensions, no MIME-type check, no magic-byte/content sniffing, no file-size limit enforced in code | all three upload sites |
| Retrieval | Direct static hyperlink: `"~/Uploads/" + AttachmentName` (or `"Uploads/" + AttachmentName` client-side in `visit_planner.aspx`'s view modal) | `visit_planner.aspx` (view modal), `vw_dailyrpts.aspx` (`hlCurrentAttachment`), `srch_dailyrpts.aspx` (`hlMegaAttachment`), and the outbound e-mail body (`GetVisitEmailBody`, hardcoded absolute URL `https://www.exc.aagroupindia.com/Uploads/{AttachmentName}`) |
| Access control on retrieval | **None.** Files are served as ordinary static content directly from the web root's `Uploads/` folder — there is no page/handler that checks `Session["USERID"]` (or ownership, or company) before returning file bytes. Anyone who obtains or guesses a filename can download the file **without logging in at all.** | inferred: no `.ashx` handler, no `Response.WriteFile` gate, no `web.config` deny-rule for `Uploads/` was found anywhere in the repository |

**Security implications:**
- **Unauthenticated, unauthorized file disclosure:** filenames are only "protected" by the second-granularity timestamp prefix plus the original filename; if an attacker knows (from a leaked chat/e-mail notification, or by observing network traffic, or by the predictable pattern itself) or brute-forces a filename, they can retrieve potentially sensitive business documents/photos without any session at all.
- **No extension restriction on upload:** nothing in the code prevents a user from uploading a file named e.g. `report.aspx`, `shell.asp`, `web.config`, or any other server-executable/parseable extension into a folder physically inside the web application root. No `Uploads/web.config` (or equivalent IIS handler-mapping override) exists in this repository to explicitly disable script execution for that folder (confirmed via a targeted search — none found). **Whether this is exploitable for remote code execution depends entirely on IIS/application-pool configuration outside this repository's visibility** — this is flagged as a **probable** (not confirmed) defect pending an infrastructure-level review; see `05_Potential_Defects.md` D-09.
- **Path/identity collision risk:** because the timestamp granularity is per-second and multiple upload sites share the exact same naming convention independently, two uploads within the same second with the same original filename would collide and silently overwrite one another (low likelihood, but zero collision detection exists).

### G.2 Expense attachments

| Aspect | Behavior |
|---|---|
| Storage path | `Server.MapPath("~/Uploads/Expenses/")` (`expense_entry.aspx.cs`) |
| Filename generation | `"EXP_" + yyyyMMddHHmmss + "_" + Path.GetFileName(...)` |
| Retrieval | `srch_dailyrpts.aspx`: `NavigateUrl='<%# Eval("AttachmentName", "~/Uploads/Expenses/{0}") %>'` — same static, unauthenticated retrieval pattern as visit attachments |

Same security implications as G.1 apply identically.

---

## J. Edit Locking — Rules That Make a Visit Editable / Non-Editable

All edit-lock logic lives in **`vw_dailyrpts.aspx.cs :: LoadMegaModal`** (the salesperson's own "My Sales Visits" edit UI). `srch_dailyrpts.aspx` never allows editing of visit *details* — the manager view only ever approves/rejects and comments, never edits the underlying fields.

### J.1 Client/UI-computed lock conditions (`LoadMegaModal`)

Evaluated in this exact order — the **first** matching condition wins and sets the lock message:

1. **`ApprovalStatus != "Pending"`** → locked. Message: `"This file is locked because it has been {ApprovalStatus}."`
2. *(else)* **`(DateTime.Now - VisitDate).TotalDays > 45`** → locked. Message: `"This file is locked because it is older than 45 days."`
   - Uses `VisitDate` (the planned/visit date), not `ExecutionDateTime` or `CreatedDate`. For a `Planned` visit whose `VisitDate` is in the future, this subtraction is negative and can never exceed 45, so this condition is effectively **only ever true for past/executed visits** — worth confirming this is intentional (see `06_Business_Rules_Requiring_Confirmation.md`).
3. *(else)* **A Manager has already left at least one comment** (`SELECT COUNT(*) FROM tbl_SalesVisitResponses WHERE VisitId=v.Id AND RespondentRole='Manager'` > 0) → locked. Message: `"Editing is locked because a Manager has already reviewed and commented on this file."`
4. Otherwise → editable.

The result (`isEditable`) drives `pnlEditForm.Enabled`, `btnUpdateVisit.Visible`, and the warning label — **this is UI presentation state only.**

### J.2 Server-side re-enforcement at update time (`btnUpdateVisit_Click`)

```sql
UPDATE tbl_SalesVisitReport
SET ...
WHERE Id = @Id
  AND ApprovalStatus = 'Pending'
  AND NOT EXISTS (SELECT 1 FROM tbl_SalesVisitResponses WHERE VisitId = tbl_SalesVisitReport.Id AND RespondentRole = 'Manager')
```

This re-validates conditions **(1)** and **(3)** from J.1 at the database level (a `0`-row update silently fails and the UI shows `"Update failed. The file may be locked by a Manager."`). **Condition (2) — the 45-day age lock — is never re-checked in this `WHERE` clause.** Combined with the complete absence of an ownership (`CreatedByCode`) check on this same `UPDATE` (see Section E.1), this means:
- The 45-day lock can be trivially bypassed by anyone able to submit the postback directly (e.g., by re-enabling the disabled panel via browser devtools, or replaying/crafting the form post), since the server does not independently verify visit age.
- The lack of an ownership check means this bypass — and indeed the update capability itself — is not even limited to the visit's own creator.

### J.3 Summary Table

| Condition | Enforced in UI? | Enforced in server `UPDATE` `WHERE` clause? |
|---|:---:|:---:|
| `ApprovalStatus = 'Pending'` | ✅ | ✅ |
| Visit age ≤ 45 days | ✅ | ❌ |
| No manager comment exists | ✅ | ✅ |
| Visit belongs to the requesting user | *(implicitly, via grid row source only)* | ❌ |
| Visit belongs to the requesting user's company | ❌ | ❌ |

---

## K. Email — Triggers, Recipients, SMTP Configuration Source, Security Issues

### K.1 Triggers and Recipients

| Trigger | File :: Method | Recipient Resolution | Subject |
|---|---|---|---|
| Salesperson or Manager sends a chat reply | `vw_dailyrpts.aspx.cs :: SendChatEmailNotification` | Always routes to the **Manager** (`Creator.ReportingManagerId → Manager.Email`) — this file only ever sends "Salesperson Reply" notifications, since chat here is always authored by the Salesperson | `"Sales Visit Report - Salesperson Reply"` |
| Chat reply (Manager dashboard) | `srch_dailyrpts.aspx.cs :: SendChatEmailNotification` | Routes to **Salesperson's** email if `senderRole == "Manager"`, or to the **Manager's** email if `senderRole == "Salesperson"` (role determined by `GetUserRole`, see E.1) | `"Sales Visit Report - Manager Reply"` or `"... - Salesperson Reply"` |
| Manager approves/rejects a visit | `srch_dailyrpts.aspx.cs :: SendApprovalNotification` | Routes to the **visit creator's** email (`tbl_login.Email` via `CreatedByCode` join) | `"Sales Visit & Expenses - {Approved|Rejected} (ID: {visitId})"` |

**Notification content:** all three build a shared-looking (but independently duplicated — see `05_Potential_Defects.md`) HTML e-mail via a `GetVisitEmailBody`-style helper (one copy per `.cs` file) that embeds the full visit record, the entire chat history, and a hardcoded absolute attachment link (`https://www.exc.aagroupindia.com/Uploads/{AttachmentName}` — a **third**, different domain from the SMTP `From` address's domain and from the login/SMTP-helper domain used elsewhere in the application, see K.3).

**Recipient validation inconsistency:**
- `srch_dailyrpts.aspx.cs`'s `SendChatEmailNotification` **and** `SendApprovalNotification` both validate the resolved e-mail with a regex (`^[^@\s]+@[^@\s]+\.[^@\s]+$`) before sending, and return silently if it fails.
- `vw_dailyrpts.aspx.cs`'s `SendChatEmailNotification` **does not** perform this regex validation — it only checks `string.IsNullOrWhiteSpace(emailTo)`. A malformed (but non-blank) manager e-mail address in `tbl_login.Email` would cause `MailMessage`/`SmtpClient` to throw inside a method whose caller (`btnMegaSendChat_Click`) does **not** wrap the call in a `try/catch` — an uncaught exception here would surface as an unhandled ASP.NET error to the salesperson simply for sending a chat message. See `05_Potential_Defects.md` D-07.

### K.2 SMTP Configuration Source (Sales Visit workflow specifically)

Both `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` construct SMTP settings via **literal, hardcoded values directly in source code**, e.g.:

```csharp
using (SmtpClient smtp = new SmtpClient("smtp.zoho.in", 587))
{
    smtp.Credentials = new NetworkCredential("[REDACTED-EMAIL-ADDRESS]", "[REDACTED-PASSWORD]");
    smtp.EnableSsl = true;
    smtp.Send(mail);
}
```
*(Redacted here per instructions; the literal sender address and password are present in plain text in both `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs`, and are therefore committed to source control history.)*

### K.3 Architectural Inconsistency: Two Different SMTP Configuration Strategies Coexist

`index.aspx.cs :: SendEmail` (used for login/password-reset/OTP flows) correctly externalizes all SMTP settings to configuration:

```csharp
string fromAddress = ConfigurationManager.AppSettings["SmtpFrom"];
string smtpUser   = ConfigurationManager.AppSettings["SmtpUser"];
string smtpPass   = ConfigurationManager.AppSettings["SmtpPass"];
string smtpHost   = ConfigurationManager.AppSettings["SmtpHost"];
int smtpPort      = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
bool enableSsl    = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpEnableSsl"]);
```

The Sales Visit workflow's two files do **not** use this pattern at all — they hardcode a **different** mail account/host directly in code. This means:
- Rotating the Sales-Visit-workflow's mailer credential requires a **code change and redeploy**, not a configuration change.
- The hardcoded credential is **exposed in the git history** of this repository to anyone with read access, independent of `Web.config` protection mechanisms (e.g. `configProtectionProvider` encryption, which — separately — does not appear to be in use for the `connectionStrings` section either, based on the plain-text connection strings observed in `Web.config`).

**Recommendation (documentation only, no code changes made per task scope):** rotate the exposed SMTP credential and migrate both files to the same `ConfigurationManager.AppSettings`-based pattern already used by `index.aspx.cs`.

### K.4 Silent Failure Pattern

Both `SendChatEmailNotification` and `SendApprovalNotification` (in both files) wrap their entire body in `catch (Exception) { /* Fail(s) silently ... */ }`. This means:
- If the SMTP send fails for **any** reason (network, credential rotation/expiry, recipient rejection, DNS), the user performing the approve/reject/chat action receives **no error and no indication whatsoever** that the notification never went out — the underlying business action (chat insert, approval status change) still succeeds and reports success to the user. This is a **confirmed** silent-failure defect for a business-critical notification path; see `05_Potential_Defects.md` D-10.
