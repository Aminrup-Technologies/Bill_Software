# FLMX Wave 1 — PR #120 Exception-Disclosure UAT

**Mode:** Controlled localhost UAT — safe UI only; no business-data mutations  
**Date (Asia/Calcutta):** 2026-11-17  
**PR:** [#120](https://github.com/Aminrup-Technologies/Bill_Software/pull/120) exception-disclosure (MyLeaves + EmpConfig)  
**Branch:** `cursor/exception-disclosure-myleaves-empconfig-1a9d`  
**Implementation HEAD:** `f188a645995c46afb135d57a6a1813f186ac81bd`  
**Host:** IIS Express `:1216` · `flamex_uat` · CompanyID **1** · ADMIN login  

## Disposition (do not overstate)

| Gate | Status |
|------|--------|
| Static exception-disclosure checks (6 owned files; zero `ex.Message` / `Exception.Message`) | **PASS** |
| Six-page localhost UI navigation (MyLeaves + EmpConfig ShiftSetup / ShiftAssignment / LeaveSetup / LeaveBalance / HolidaySetup) | **PASS** |
| ShiftSetup `tran.Rollback(); throw;` preservation | **PASS** |
| Visual Studio debugger attachment and Debug output capture capability | **PASS** |
| Specific modified catch-block `Debug.WriteLine(ex.ToString())` emission under a forced fault | **NOT VERIFIED** |
| Business-data mutations during verification | **None** |
| **Overall disclosure UAT** | **PASS WITH QUALIFICATION** |

Qualification: VS Debug output capture was available, but this run did **not** force a failure through the modified catch sites to observe those exact `Debug.WriteLine` lines. Static presence of `Debug.WriteLine(ex.ToString())` + controlled UI strings is confirmed; runtime emission of those specific catch blocks is not claimed.

---

## Owned files (implementation HEAD)

- `Bill_Software/corporate/business/app/MyLeaves.aspx.cs`
- `Bill_Software/corporate/business/app/AdminShiftSetup.aspx.cs`
- `Bill_Software/corporate/business/app/AdminShiftAssignment.aspx.cs`
- `Bill_Software/corporate/business/app/AdminLeaveSetup.aspx.cs`
- `Bill_Software/corporate/business/app/AdminLeaveBalance.aspx.cs`
- `Bill_Software/corporate/business/app/AdminHolidaySetup.aspx.cs`

Controlled UI strings (no exception text): `"Unable to complete this request. No changes were saved."` / `"Unable to load data."` (ShiftAssignment uses static Swal strings).

---

## Safe UI validation (`flamex_uat`, CompanyID 1)

Login: ADMIN / `admin`. Navigation/load only — no Save / Allocate / submit / approve.

| Page | Result | Exception leak on UI |
|------|--------|----------------------|
| MyLeaves | **PASS** (loaded) | none |
| AdminShiftSetup | **PASS** (loaded) | none |
| AdminShiftAssignment | **PASS** (loaded) | none |
| AdminLeaveSetup | **PASS** (loaded) | none |
| AdminLeaveBalance | **PASS** (loaded) | none |
| AdminHolidaySetup | **PASS** (loaded) | none |

---

## Environment notes

- Local UAT served from worktree at implementation HEAD `f188a645…`.
- Tip `Web.config` at that commit named `DbConn` toward LIVE; localhost UAT used an **uncommitted** local retarget to `flamex_uat` only (not part of any documentation or implementation commit).
- No LIVE writes, no SQL mutations, no application source changes in this UAT close-out.

---

## Media (Project store)

Screenshots under Project store `media/pr120-*` (home + six-page loads). Not committed to the repository.

---

## Verdict

**PASS WITH QUALIFICATION** — static disclosure, six-page UI navigation, and ShiftSetup rollback/rethrow **PASS**; VS Debug capture capability **PASS**; specific catch-block `Debug.WriteLine` emission **NOT VERIFIED**. No business-data mutations.
