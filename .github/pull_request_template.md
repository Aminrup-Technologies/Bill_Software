## Summary

<!-- What changed and why. One short paragraph. -->

## Security Gate

- [ ] Complies with [`docs/22_Security_Baseline.md`](docs/22_Security_Baseline.md)
- [ ] Session validated (`USERID` + `SessionToken` + `ActiveSessions`)
- [ ] Company validated (`UserCompanyAccess` / `Session["CompanyID"]`)
- [ ] Permission enforced at the point of action (`SecurePage` / `AuthGuard`)
- [ ] Resource IDs authorized before mutation (view / edit / delete / approve)
- [ ] `CompanyID` is not trusted from client input
- [ ] New pages inherit `SecurePage` (or an approved exception is documented)
- [ ] New WebMethods use `EnableSession` + `EnsureWebMethodPermission`
- [ ] Protected files (`AuthGuard`, `SecurePage`, `UserRoleAssignment`, `UserCompanyAccess`) are unchanged unless this PR is an approved architecture review

## Data Gate

- [ ] All SQL is parameterized (no string-built queries with untrusted input)
- [ ] ADO.NET objects are in `using` blocks
- [ ] No new duplicated data-access pattern (reuse existing parameterized commands; do not add a parallel DAL)
- [ ] Transaction behavior is preserved (`tbl_SystemNotification` before commit where required)
- [ ] Tenant filter uses validated `CompanyID` / ownership predicate

## Regression Gate

- [ ] No business-rule changes unless explicitly documented in this PR
- [ ] Invoice / PO / stock / scheduler / sales-visit / quotation / leave workflows unchanged unless listed below
- [ ] Authentication and authorization behavior unchanged unless this is an approved security change

## Scope Gate

Intentionally **untouched** (list files or modules):

<!-- Example: AuthGuard.cs, SecurePage.cs, invoice workflow -->

-

## Files Changed

| File | Reason |
|------|--------|
| | |

## Testing

<!-- Evidence: what was run, which pages/WebMethods, UAT vs local. -->

- [ ]

## Rollback

<!-- What happens if this PR is reverted. Data/schema impact, if any. -->

-
