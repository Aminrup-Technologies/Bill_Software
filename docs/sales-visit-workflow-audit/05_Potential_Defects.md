# 05 — Potential Defects (L)

**No code, database, or configuration has been modified in the course of this audit.** Every finding below is documented for review; none have been fixed. Each finding lists: **File**, **Method**, **Relevant Code Area**, **Database Object**, **Impact**, and **Confidence** (`Confirmed` / `Probable` / `Architectural Inconsistency` / `Business-Rule Ambiguity`).

---

## CONFIRMED DEFECTS

### D-01 — `CompanyID` is never populated when a Sales Visit is created, breaking tenant-scoped features downstream
- **File / Method:** `daily_rpt.aspx.cs :: btnSubmit_Click`; `visit_planner.aspx.cs :: btnSubmitExecution_Click` (auto-follow-up `INSERT`)
- **Code area:** Both `INSERT INTO tbl_SalesVisitReport (...)` column lists omit `CompanyID` entirely. A repository-wide search found **no** `INSERT INTO tbl_SalesVisitReport` statement anywhere that sets this column.
- **Database object:** `tbl_SalesVisitReport.CompanyID`
- **Impact:** Every visit created through this workflow (both directly-planned/past-logged visits and system-generated follow-ups) will have a `NULL` (or DB-default) `CompanyID`. This directly breaks: (a) `srch_dailyrpts.aspx.cs :: Binder()`, which filters `WHERE CompanyID = <val>` — such visits will **never appear** on the manager dashboard; (b) `AdminAttendanceDashboard.aspx.cs`'s per-company field-sales rollups, which will silently under-count; (c) `Create_quotation.aspx.cs`'s `SELECT CustomerName FROM tbl_SalesVisitReport WHERE Id=@Id AND CompanyID=@CompanyID` — the "📄 Generate Quote" button wired in `visit_planner.aspx`'s Executed-visit view modal will find **zero rows** for any visit created by this workflow, silently failing to prefill the customer name on every single quote generated from a sales visit.
- **Confidence:** Confirmed (directly evidenced by comparing INSERT column lists against confirmed downstream `SELECT ... WHERE CompanyID=...` consumers in `Create_quotation.aspx.cs` and `AdminAttendanceDashboard.aspx.cs`).

### D-02 — `Status` vocabulary mismatch causes silent data loss on edit
- **File / Method:** `vw_dailyrpts.aspx` (markup, `edit_ddlStatus` items) and `vw_dailyrpts.aspx.cs :: LoadMegaModal` / `btnUpdateVisit_Click`
- **Code area:** `edit_ddlStatus` only contains items `Completed` / `Pending Execution` / `Escalated` (plus blank). The execute-flows (`daily_rpt.aspx` `ddlStatus`, `visit_planner.aspx` `ddlExecStatus`) write the literal `"Pending"` (not `"Pending Execution"`). `LoadMegaModal`'s guard `if (edit_ddlStatus.Items.FindByValue(rdr["Status"].ToString()) != null) ...` silently no-ops when the stored value is `"Pending"`, leaving the dropdown on its blank default. If the user then saves, `btnUpdateVisit_Click`'s `UPDATE ... SET Status = @Status` writes back the blank value.
- **Database object:** `tbl_SalesVisitReport.Status`
- **Impact:** A visit whose true status is `"Pending"` can be silently corrupted to an empty string the moment its owner opens the "Visit File" edit modal and saves, even if they never intended to touch the Status field. Downstream status-based filters (e.g. `vw_dailyrpts.aspx`'s own `ddlSearchStatus`) would then no longer find that record under any of its dropdown options.
- **Confidence:** Confirmed (direct code/markup comparison; reproducible from the described sequence of actions).

### D-03 — Hardcoded fallback user code `"FLM03"` silently misattributes visit ownership
- **File / Method:** `daily_rpt.aspx.cs :: btnSubmit_Click`
- **Code area:** `string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03"; cmd.Parameters.AddWithValue("@CreatedByCode", userId);`
- **Database object:** `tbl_SalesVisitReport.CreatedByCode`
- **Impact:** `Page_Load` already redirects to `index.aspx` when `Session["USERID"]` is null, so this path is *not* normally reachable on first load — but the null-coalescing fallback specifically covers the case where the session expires/is cleared **between** `Page_Load` and the `btnSubmit_Click` postback (e.g., a long-lived form left open past session timeout). In that scenario, the visit is silently attributed to a **specific, real-looking, hardcoded employee code (`FLM03`)** rather than failing the request or re-prompting for login — a session-timeout bug becomes a **data-integrity/attribution bug** (a visit shows up as created by a person who never created it) rather than a visible error.
- **Confidence:** Confirmed (defect exists exactly as coded; likelihood of triggering depends on session-timeout configuration, which is out of scope for this static review).

### D-04 — Broken access control (IDOR) across nearly every detail/mutation endpoint in this workflow
- **Files / Methods:**
  - `visit_planner.aspx.cs :: GetVisitDetails(int visitId)` — `WHERE Id=@Id` only
  - `visit_planner.aspx.cs :: btnSubmitExecution_Click` — `UPDATE ... WHERE Id=@Id` only
  - `vw_dailyrpts.aspx.cs :: LoadMegaModal` — `WHERE v.Id=@Id` only
  - `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click` — `WHERE Id=@Id AND ApprovalStatus='Pending' AND NOT EXISTS(...)` (business-rule guard only, no ownership guard)
  - `srch_dailyrpts.aspx.cs :: LoadMegaModal` — `WHERE Id=@Id` only (no `CompanyID` re-check despite the originating list being `CompanyID`-scoped)
  - `srch_dailyrpts.aspx.cs :: ProcessApproval` — `UPDATE ... WHERE Id=@Id` only (no `CompanyID`, no `ReportingManagerId` check)
  - `srch_dailyrpts.aspx.cs :: gvMegaExpenses_RowCommand` — `UPDATE tbl_Expenses ... WHERE Id=@Id` only
- **Database objects:** `tbl_SalesVisitReport`, `tbl_Expenses`
- **Impact:** None of these endpoints verify that the acting user (a) owns the target visit (`CreatedByCode`), (b) shares the target's `CompanyID`, or (c) is the target salesperson's actual `ReportingManagerId`. Since these are all reached via standard ASP.NET postback mechanisms (hidden fields, `CommandArgument`, PageMethod JSON bodies) carrying a plain integer `Id`/`visitId`, and `Id` values are sequential and therefore trivially enumerable, **any authenticated user of the application — regardless of company, role, or reporting line — can view, execute, edit, approve, or reject any other user's sales visit or expense**, simply by supplying a different `Id` in the corresponding request. This is the most severe and most widely-replicated finding in this audit.
- **Confidence:** Confirmed (directly evidenced by absence of any ownership/tenant predicate in every listed `WHERE` clause).

### D-05 — SQL Injection in the Manager Dashboard search
- **File / Method:** `srch_dailyrpts.aspx.cs :: Binder()`
- **Code area:**
  ```csharp
  cmdstring = "SELECT * FROM tbl_SalesVisitReport WHERE CompanyID = " + companyId +
              " AND CreatedByCode = '" + selectedUser + "' AND CAST(VisitDate as date) BETWEEN '" +
              fromDateStr + "' AND '" + toDateStr + "' ORDER BY CAST(VisitDate as date) DESC";
  ...
  SqlDataAdapter da = new SqlDataAdapter(cmdstring, conn); // executed as raw text, no parameters
  ```
- **Database object:** `tbl_SalesVisitReport` (arbitrary read via injection; depending on DB permissions, potentially broader)
- **Impact:** `fromDateStr`/`toDateStr` originate from free-text `TextBox` controls (`txtfromDate`/`txttodate`) that, while paired with a client-side jQuery UI datepicker, are **not** constrained server-side to a date format before being concatenated directly into SQL text. `selectedUser` originates from a server-rendered `DropDownList`, which is somewhat harder (but not proven impossible, depending on `EnableEventValidation`/`ViewState` MAC settings) to tamper with via a forged form post. This is a classic, textbook SQL injection vulnerability in a manager-facing page that already has elevated (cross-employee) data access — the same method that already suffers from the D-04 authorization gap.
- **Confidence:** Confirmed (raw string concatenation into `SqlDataAdapter`, no parameterization, directly observable in code). A secondary, lower-severity instance of the same anti-pattern exists in `daily_rpt.aspx.cs :: GetAdminName()` (`"select Name from tbl_login where User_Id='" + UserName + "'"`), though there the input is the session's own server-set `USERID` rather than direct user input, reducing (but not eliminating, e.g. under session-fixation) exploitability.

### D-06 — `ParentVisitId` is written but never read anywhere in the application
- **File / Method:** `visit_planner.aspx.cs :: btnSubmitExecution_Click` (writes it); no method anywhere reads it
- **Code area:** `INSERT INTO tbl_SalesVisitReport (..., ParentVisitId) ... SELECT ..., @Id FROM tbl_SalesVisitReport WHERE Id=@Id`
- **Database object:** `tbl_SalesVisitReport.ParentVisitId`
- **Impact:** The application persists a follow-up lineage relationship that has no corresponding read path, report, or UI affordance anywhere in the analyzed workflow (or its immediate cross-file dependents). Users cannot see "this visit was auto-generated from visit #123" nor "visit #123 generated this follow-up" anywhere in the product. This is dead functionality from the user's perspective — data is captured with no way to consume it.
- **Confidence:** Confirmed (verified by exhaustive search for `ParentVisitId` across the codebase — write-only).

### D-07 — Missing recipient-email validation + missing exception handling around chat notification in `vw_dailyrpts.aspx.cs`
- **File / Method:** `vw_dailyrpts.aspx.cs :: SendChatEmailNotification`, called from `btnMegaSendChat_Click`
- **Code area:** `if (string.IsNullOrWhiteSpace(emailTo)) return;` (no regex/format validation, unlike the sibling implementation in `srch_dailyrpts.aspx.cs`); `btnMegaSendChat_Click` calls `SendChatEmailNotification(...)` **without** wrapping it in a `try/catch` (contrast: `srch_dailyrpts.aspx.cs`'s equivalent method has its own internal `try/catch` around the whole body, so this specific gap is unique to the `vw_dailyrpts.aspx.cs` copy... actually both have internal try/catch — the more precise issue is the missing regex check allowing a malformed-but-non-blank address to reach `MailAddress`/`SmtpClient`, which **would** be caught by the method's own `catch (Exception) { /* Fail silently */ }` — meaning the real, distinct impact is that a malformed manager e-mail causes the notification to fail *silently* rather than throwing (see D-10), while `srch_dailyrpts.aspx.cs`'s stricter regex at least prevents attempting the send at all.
- **Database object:** `tbl_login.Email` (data quality dependency)
- **Impact:** Inconsistent behavior between the two near-duplicate chat-notification implementations for the same logical feature; the salesperson-facing copy is more permissive/less defensive than the manager-facing copy, for no apparent intentional reason — evidence of copy-paste code drift rather than a shared, tested helper.
- **Confidence:** Confirmed (direct comparison of the two nearly-identical method bodies).

### D-08 — No idempotency / concurrency guard on visit approval
- **File / Method:** `srch_dailyrpts.aspx.cs :: ProcessApproval`
- **Code area:** `UPDATE tbl_SalesVisitReport SET ApprovalStatus=@Status, ManagerRemarks=@Remarks, ApprovedDate=GETDATE(), ApprovedBy=@User WHERE Id=@Id` — no `AND ApprovalStatus='Pending'` predicate (contrast with `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`, which *does* include such a guard for its own, different, update).
- **Database object:** `tbl_SalesVisitReport.ApprovalStatus`, `.ManagerRemarks`, `.ApprovedDate`, `.ApprovedBy`
- **Impact:** Two managers (or one manager double-submitting, e.g. via a slow network + repeated click, or a replayed request) acting on the same visit within a short window can each overwrite the other's decision/remarks/timestamp with no conflict detection, no error, and no audit trail of the overwritten prior decision. The already-approved/rejected visit can also be silently re-approved/re-rejected by a second action even though the UI intends to hide the buttons once actioned (client-side-only enforcement).
- **Confidence:** Confirmed (absence of any status predicate directly observable in the `UPDATE` statement).

### D-09 — Uploaded attachments (visit & expense) accept any file extension and are served from an unauthenticated static path
- **Files / Methods:** `visit_planner.aspx.cs :: btnSubmitExecution_Click`, `daily_rpt.aspx.cs :: btnSubmit_Click`, `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`, `expense_entry.aspx.cs :: btnSubmit_Click` (all four independent upload sites)
- **Code area:** `fileName = <timestamp> + "_" + Path.GetFileName(<upload>.FileName); ...SaveAs(Path.Combine(uploadPath, fileName));` — no extension allow/deny-list, no content-type check, no size limit in code.
- **Database object:** `tbl_SalesVisitReport.AttachmentName`, `tbl_Expenses.AttachmentName` (filenames only; files live on disk under `~/Uploads/` and `~/Uploads/Expenses/`)
- **Impact:** (a) Retrieval is via a direct, unauthenticated static link (`~/Uploads/<name>`) — no page/handler checks `Session["USERID"]` before serving the file, so anyone with the URL (guessed, leaked via a forwarded e-mail notification, or observed) can download it without logging in. (b) No repository evidence of an `Uploads/web.config` (or equivalent) disabling script execution for that folder was found — whether an uploaded file with a server-executable extension could be invoked directly depends on IIS/application-pool handler mappings outside this repository's visibility, so the remote-code-execution angle is flagged as **Probable**, not Confirmed, pending an infrastructure-level review; the **unauthenticated-disclosure** angle, however, is Confirmed purely from application code.
- **Confidence:** Confirmed (unauthenticated disclosure); Probable (script-execution/RCE angle, contingent on IIS configuration not visible in this repository).

### D-10 — Notification e-mail failures are always swallowed silently
- **Files / Methods:** `vw_dailyrpts.aspx.cs :: SendChatEmailNotification`; `srch_dailyrpts.aspx.cs :: SendChatEmailNotification`, `SendApprovalNotification`
- **Code area:** each method's entire body is wrapped in `catch (Exception) { /* Fail(s) silently ... */ }`
- **Database object:** n/a (notification/integration concern)
- **Impact:** If the hardcoded SMTP credential is rotated/expired, the SMTP host becomes unreachable, or the recipient address is rejected, the triggering business action (approval, rejection, chat reply) still reports success to the acting user with **zero indication** that the corresponding notification never reached its recipient. Given that approvals/rejections and manager chat replies are the primary way a salesperson learns the outcome of their submitted visit, a prolonged, invisible SMTP outage would silently break a core communication path of the workflow.
- **Confidence:** Confirmed (directly observable exception-swallowing in all three methods).

### D-11 — Hardcoded, source-committed SMTP credentials, inconsistent with the rest of the application's configuration pattern
- **Files / Methods:** `vw_dailyrpts.aspx.cs :: SendChatEmailNotification`, `GetVisitEmailBody`; `srch_dailyrpts.aspx.cs :: SendChatEmailNotification`, `SendApprovalNotification`, `GetVisitEmailBody`
- **Code area:** literal `new NetworkCredential(<email>, <password>)` and literal `new SmtpClient("smtp.zoho.in", 587)` — full detail and redaction rationale in `04_Security_and_Tenant_Audit.md` §K.2–K.3.
- **Database object:** n/a
- **Impact:** Credential is present in plaintext in source control history; rotating it requires a code change/redeploy rather than a configuration change; directly contradicts the externalized-configuration pattern already used elsewhere in the same application (`index.aspx.cs :: SendEmail`, which reads `ConfigurationManager.AppSettings["Smtp*"]`).
- **Confidence:** Confirmed (credential literal directly present in source, redacted in this report per instructions).

---

## PROBABLE DEFECTS

### D-12 — Non-atomic two-statement execution + follow-up creation
- **File / Method:** `visit_planner.aspx.cs :: btnSubmitExecution_Click`
- **Code area:** a single `SqlCommand` containing an `UPDATE` followed by a conditional `INSERT`, with no explicit `BEGIN TRANSACTION ... COMMIT`/`SqlTransaction` wrapping either statement.
- **Database object:** `tbl_SalesVisitReport`
- **Impact:** While SQL Server will execute both statements as part of a single batch, without an explicit transaction each statement auto-commits independently under default connection settings. If the `INSERT` fails for any reason after the `UPDATE` has already succeeded (e.g., a constraint violation on the copied data, or a transient error), the source visit would be left `Executed`/GPS-tagged/attachment-saved with **no** follow-up row created, silently violating the "if `FollowUpRequired='Yes'` and a next date is given, a follow-up **will** be created" expectation, with no error surfaced distinguishing this partial-failure case from a full success (the generic `catch` only reports `ex.Message` in an alert if an exception propagates at all).
- **Confidence:** Probable (the failure mode requires a secondary error during the `INSERT`, which is not certain to occur, but the lack of explicit transactional wrapping is a clear code-level gap regardless of whether it has yet manifested in production).

### D-13 — No server-side re-validation of required fields (defense-in-depth gap)
- **Files / Methods:** `daily_rpt.aspx.cs :: btnSubmit_Click`; `visit_planner.aspx.cs :: btnSubmitExecution_Click`; `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`
- **Code area:** absence of any server-side equivalent to the client-side `validateSalesVisitForm()` / `captureLocationAndSubmit()` required-field checks.
- **Database object:** `tbl_SalesVisitReport` (multiple `nvarchar` columns)
- **Impact:** Because ASP.NET WebForms postbacks can be crafted directly (bypassing the page's own JavaScript), a request that omits Customer Name, Department, Contact Person, Discussion Points, etc. would still succeed at the database layer (these are permissive `nvarchar` columns with no `NOT NULL`/`CHECK` constraint evidenced), producing incomplete records that would otherwise have been blocked by the UI.
- **Confidence:** Probable (exploitability depends on whether the underlying columns are nullable/unconstrained at the DB level, which cannot be fully confirmed without direct schema access — but the *application-layer* gap itself is Confirmed).

### D-14 — Raw exception messages surfaced directly to end users
- **Files / Methods:** `daily_rpt.aspx.cs :: btnSubmit_Click` (`lblErrorMsg.Text = "An error occurred: " + ex.Message;`); `visit_planner.aspx.cs :: btnSubmitExecution_Click` (`Response.Write("<script>alert('Error: " + ex.Message + "');</script>");`); `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`; `srch_dailyrpts.aspx.cs :: ProcessApproval`, `Binder()`
- **Code area:** generic `catch (Exception ex)` blocks that echo `ex.Message` directly into UI labels or inline `<script>alert(...)</script>` blocks.
- **Database object:** n/a
- **Impact:** Raw ADO.NET/SQL exception text (which can include fragments of table/column names, constraint names, or query structure) is exposed to end users, aiding reconnaissance for the SQL-injection vector already identified in D-05, and providing a poor/unprofessional user experience.
- **Confidence:** Probable (information-disclosure severity depends on what SQL Server actually includes in exception messages for a given failure, which varies by error type).

---

## ARCHITECTURAL INCONSISTENCIES

### D-15 — Two incompatible tenancy philosophies within one workflow
Self-service pages (`visit_planner.aspx`, `vw_dailyrpts.aspx`) scope by `CreatedByCode` only; the manager page (`srch_dailyrpts.aspx`) scopes its list by `CompanyID` only; **no page's detail/mutation endpoints enforce either.** Full detail in `04_Security_and_Tenant_Audit.md` §E.2. **Confidence:** Confirmed as an inconsistency (the divergence itself is directly observable); its *root cause* — an incomplete backport of the "Full-Stack CompanyContext segregation fix" applied elsewhere (`Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs`) — is inferred from the matching code comment found in those other files.

### D-16 — `ReportingManagerId` is a routing concept, not an authorization concept
The manager hierarchy column exists and is actively used for e-mail routing, but is never used to determine who is *allowed* to approve, reject, comment on, or view a given salesperson's visit file. Full detail in `04_Security_and_Tenant_Audit.md` §E.1 and D-04 above. **Confidence:** Confirmed (the column's sole usage site is the e-mail-routing `LEFT JOIN`).

### D-17 — Follow-up auto-generation is implemented in exactly one of three plausible trigger points
Automatic follow-up-visit creation only happens via `visit_planner.aspx.cs :: btnSubmitExecution_Click`; it does not happen when a past visit is logged directly with `FollowUpRequired='Yes'` (`daily_rpt.aspx`, mode=past), nor when an existing visit's follow-up fields are edited after the fact (`vw_dailyrpts.aspx`). Full detail in `03_State_Machine.md` §I. **Confidence:** Confirmed as a code-level inconsistency; whether it represents a *bug* or an *intentional scope limitation* is a business-rule question — carried forward to `06_Business_Rules_Requiring_Confirmation.md`.

### D-18 — Duplicated, drifted business logic across `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs`
Chat insertion, chat-history rendering, `GetVisitEmailBody` HTML generation, and e-mail sending are each implemented **twice** — once per file — with near-identical but not identical code (see D-07 for one concrete behavioral divergence this has already produced). No shared helper/service class is used. **Confidence:** Confirmed (two independent, near-duplicate method bodies observed for each of the listed responsibilities).

### D-19 — Three different data-entry surfaces write overlapping fields on the same entity with different defaults/vocabularies
`daily_rpt.aspx` (plan and past modes), `visit_planner.aspx`'s execute modal, and `vw_dailyrpts.aspx`'s edit modal each independently define their own `Status`/`FollowUpRequired` dropdown option lists and hardcoded defaults, rather than sharing a single canonical list — the direct root cause of D-02. **Confidence:** Confirmed (directly observable across the three `.aspx` markup files).

---

## BUSINESS-RULE AMBIGUITIES (technical framing — see `06_Business_Rules_Requiring_Confirmation.md` for the full, standalone list posed as questions)

- Is GPS capture intended to be mandatory for **all** visit executions, or only for those executed live via the calendar UI? (`daily_rpt.aspx?mode=past` never requests it.)
- Is the 45-day edit-lock window intended to apply only to `Executed`/past visits (as its current `VisitDate`-based math implies), or should it also eventually apply to long-stale `Planned` visits?
- Should `ProcessApproval` require non-empty Manager Remarks, given the UI's placeholder text implies this is expected?
- Is it intentional that visit approval no longer cascades to bulk-approve linked expenses (per the `// (REMOVED the bulk tbl_Expenses update from here)` comment), or was that a regression?
- What is the intended terminal state for a `Planned` visit that is never executed and never explicitly cancelled?
