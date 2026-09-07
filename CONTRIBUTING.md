# Contributing to Project_FLMX

Canonical security contract: [`docs/22_Security_Baseline.md`](docs/22_Security_Baseline.md).  
Release baseline: `v2.1-security-foundation`.

> New modules inherit `AuthGuard` and `SecurePage`; they do not implement parallel authentication or authorization.

---

## 1. Branch Strategy

- **`master`** is the production trunk.
- **`July_to_Sept26_DevNSupport`** tracks the same HEAD as `master` (integration line).
- Create feature branches from **`master`**.
- Do not rewrite `v2.1-security-foundation` history.
- Do not change `AuthGuard`, `SecurePage`, `UserRoleAssignment`, or `UserCompanyAccess` without architectural review (see `.github/CODEOWNERS`).

## 2. PR Process

1. Open a PR against `master` using the repository pull-request template.
2. Complete all four gates: **Security**, **Data**, **Regression**, **Scope**.
3. Keep the change scoped. List modules you intentionally did not touch.
4. Include test evidence and rollback impact.
5. Merge with a **merge commit** unless a later program says otherwise. Do not squash the Security Foundation history.

## 3. Security Baseline Requirement

Every PR that touches pages, WebMethods, print, SQL, session, or configuration must satisfy [`docs/22_Security_Baseline.md`](docs/22_Security_Baseline.md):

- Authentication → ActiveSessions → company membership → page permission → resource authorization → business operation.
- Menu visibility is not authorization.
- `CompanyID` is never trusted from client input.
- Kiosk (`tbl_card_login`) stays isolated from ERP auth.

Protected assets: `AuthGuard.cs`, `SecurePage.cs`, `UserRoleAssignment.cs`, `UserCompanyAccess.sql`, `docs/22_Security_Baseline.md`.

## 4. Data Access Standard

- Parameterize all SQL. No concatenation of identifiers or tenant values.
- Use `using` blocks for `SqlConnection`, `SqlCommand`, and `SqlDataReader`.
- Do not introduce a second data-access stack alongside existing ADO.NET.
- Preserve transaction and `tbl_SystemNotification` behavior.
- Filter tenant data with validated `Session["CompanyID"]` / `CompanyContext.CurrentCompanyID` (or the documented ownership predicate).

## 5. Regression Rule

Do not change business rules unless the PR states that change explicitly.

Do not migrate to ASP.NET Identity or JWT. Do not start another AuthN/AuthZ rewrite. Decision #8 (`ReportingManagerId`), Decision #9 (HQ cross-company), kiosk isolation, `machineKey` rotation, and secret cutover remain documented boundaries.
