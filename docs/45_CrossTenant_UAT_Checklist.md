# Cross-Tenant Duplication — UAT Evidence Checklist

**Date:** September 14, 2026
**Feature:** PRs #85–#88 (Cross-Tenant Duplication Infrastructure)
**Branches:** `feat/cross-tenant-duplication-pr1` through `pr4`
**Base:** `July_to_Sept26_DevNSupport`

---

## How to Use This Checklist

1. Log into the application as a **multi-company user** (one with access to ≥ 2 companies).
2. Navigate to **View Vendors** (`View_vendor.aspx`) or **View Clients** (`View_client.aspx`).
3. Execute each test case below in order.
4. Record evidence in the **Evidence** column: screenshot filename, DB query result, or observation.
5. Mark **Pass/Fail** in the **Result** column.

---

## Preconditions

| # | Check | Expected | Evidence | Result |
|---|-------|----------|----------|--------|
| P-1 | User has access to at least 2 companies (Company A = current, Company B = target) | Company dropdown shows ≥ 1 target option | | |
| P-2 | Company A has ≥ 1 vendor with child data (GST, PAN, bank) | Vendor list shows data | | |
| P-3 | Company A has ≥ 1 customer with child records (address, factory, representative) | Client list shows data | | |
| P-4 | Company B has at least 1 existing vendor (for name-collision test) | Note the Vendor_Name of an existing vendor in Company B | | |
| P-5 | Company B has at least 1 existing customer (for name-collision test) | Note the Client_Name of an existing customer in Company B | | |

---

## A. Vendor Single Duplication

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| V-01 | Basic duplicate | 1. Click 📋 Duplicate on a vendor row 2. Modal opens with company dropdown 3. Select Company B 4. Click "Confirm Duplicate" 5. Confirm dialog appears 6. Click OK | Success message: "Vendor 'AA##' duplicated successfully to 'Company B'". Vendor appears in Company B's list with new AA## code. | | |
| V-02 | New Vendor_Id generation | After V-01, query Company B's last Vendor_Id | New Vendor_Id = MAX(existing) + 1, with "AA" prefix, scoped to Company B. | | |
| V-03 | All master fields copied | Compare source and target vendor row in DB | Vendor_Name, Address1, Address2, City, pin, State, Com_phone, Com_email, Com_Fax, Rep_Name, Rep_Desig, Rep_phone, Rep_email, Service_tax_No, Pan_No, Vat_No, PrincipleVndrCode, BankAccNo, BankIfscCode, AccountName — all match source. | | |
| V-04 | CompanyID is target | `SELECT CompanyID FROM tbl_Vendor WHERE Vendor_Id = '<new_code>'` | CompanyID = Company B's ID (NOT Company A's). | | |
| V-05 | CreatedBy set | `SELECT CreatedBy FROM tbl_Vendor WHERE Vendor_Id = '<new_code>'` | CreatedBy = logged-in User_Id. | | |
| V-06 | Source unchanged | Query Company A's original vendor | Original Vendor_Id, all fields, and CompanyID are unchanged. | | |
| V-07 | Purchase history NOT copied | `SELECT COUNT(*) FROM tbl_Purches WHERE Vendor_Id = '<new_code>'` | 0 rows (purchases are not duplicated). | | |
| V-08 | Audit entry written | `SELECT TOP 1 * FROM tbl_SystemNotification WHERE CompanyID = <CompanyB_ID> AND Title = 'Vendor Duplicated' ORDER BY CreatedOn DESC` | Row exists with: CompanyID = Company B, Message contains source code + new code + acting user, UserId = logged-in user. | | |

---

## B. Vendor Bulk Duplication

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| VB-01 | Select multiple vendors | 1. Check 3 vendor checkboxes 2. Click "📋 Bulk Duplicate Selected" 3. Modal opens showing "Duplicate 3 vendor(s)..." | 3 vendor IDs stored in hidden field. Modal opens. | | |
| VB-02 | Select All | 1. Click header "Select All" checkbox 2. Verify all row checkboxes checked | All visible vendor checkboxes are checked. | | |
| VB-03 | Clear All | 1. Uncheck header checkbox 2. Verify all row checkboxes unchecked | All row checkboxes cleared. | | |
| VB-04 | Bulk duplicate success | 1. Check 3 vendors 2. Click Bulk Duplicate 3. Select Company B 4. Confirm | Success message: "3 vendor(s) duplicated". All 3 appear in Company B with new AA## codes. | | |
| VB-05 | Bulk audit entries | `SELECT COUNT(*) FROM tbl_SystemNotification WHERE CompanyID = <B> AND Title = 'Vendor Duplicated' AND CreatedOn > '<test_start>'` | ≥ 3 rows (one per duplicated vendor). | | |
| VB-06 | Zero selection blocked | 1. Don't check any vendors 2. Click Bulk Duplicate | JS alert: "Please select at least one vendor to duplicate." Postback does NOT occur. | | |
| VB-07 | Button text feedback | During processing | Button text changes to "Duplicating..." and is disabled. | | |
| VB-08 | Button recovery after error | Trigger a failure (e.g., duplicate code after 5 retries) | Button re-enabled and text restored to "Confirm Duplicate" via `finally` block. | | |

---

## C. Customer Single Duplication

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| C-01 | Basic duplicate | 1. Click 📋 Duplicate on a client row 2. Select Company B 3. Confirm | Success message: "Client 'AD##' and child records duplicated successfully to 'Company B'". | | |
| C-02 | New Client_Id generation | After C-01, query Company B | New Client_Id = MAX(existing) + 1 with "AD" prefix, scoped to Company B. | | |
| C-03 | All master fields copied | Compare source and target | Client_Name, Industry, Address1, State, City, pin, Com_phone, Com_Fax, Com_web_site, Com_email, Service_tax_no, Pan_no, PlaceofSupply — all match source. | | |
| C-04 | CompanyID is target | `SELECT CompanyID FROM tbl_Client WHERE Client_Id = '<new_code>'` | CompanyID = Company B. | | |
| C-05 | CreatedBy set | `SELECT CreatedBy FROM tbl_Client WHERE Client_Id = '<new_code>'` | CreatedBy = logged-in User_Id. | | |
| C-06 | Source unchanged | Query Company A's original client | Original record unchanged. | | |
| C-07 | Audit entry written | `SELECT TOP 1 * FROM tbl_SystemNotification WHERE CompanyID = <B> AND Title = 'Customer Duplicated'` | Row exists with full context. | | |

---

## D. Customer Child Entity Cloning

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| D-01 | tbl_ClientRegAddress cloned | `SELECT * FROM tbl_ClientRegAddress WHERE Client_Id = '<new_AD>'` | All address rows from source copied with new Client_Id. Address1, Address2, City, State, pin, ContactPerson, Phone, Email match source. | | |
| D-02 | tbl_Factory cloned | `SELECT * FROM tbl_Factory WHERE Client_id = '<new_AD>'` | All factory rows copied. Factory_name, Address1, Address2, city, State, pin match source. CompanyID = Company B. | | |
| D-03 | tbl_representative cloned | `SELECT * FROM tbl_representative WHERE Copany_Id = '<new_AD>'` | All representative rows copied. RepTitle, Representative_name, RepLastName, Designation, Phone_no, Email match source. CompanyID = Company B. | | |
| D-04 | Child CompanyID correct | For each cloned child table | All CompanyID values = Company B (not Company A). | | |
| D-05 | Transactional tables NOT copied | `SELECT COUNT(*) FROM tbl_Quotation WHERE Client_Id = '<new_AD>'` | 0 rows. Same for tbl_Invoice, tbl_Chalan, tbl_Proforma, tbl_Purches. | | |
| D-06 | Source children unchanged | Query Company A's original Client_Id in child tables | All original child records unchanged. | | |

---

## E. Customer Bulk Duplication

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| CB-01 | Bulk duplicate 3 clients | 1. Check 3 client checkboxes 2. Click Bulk Duplicate 3. Select Company B 4. Confirm | Success message with count. All 3 clients + children appear in Company B. | | |
| CB-02 | Child entities cloned for bulk | For each of the 3 duplicated clients | All 3 child tables (Address, Factory, Representative) are cloned. | | |
| CB-03 | Bulk rollback on failure | (Requires DBA setup: temporarily add a CHECK constraint or trigger to force failure on one INSERT) | If one client fails, the ENTIRE batch rolls back. No partial duplication. Original data unchanged. | | |

---

## F. Rollback / Error Handling

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| R-01 | Transaction rollback | Force a failure mid-transaction (e.g., INSERT a row with a duplicate UNIQUE key in Company B manually, then duplicate into it) | Exception caught, transaction rolled back. Source data unchanged. Error message shown to user. | | |
| R-02 | Code generation exhaustion | (Requires DBA setup: manually create 5 consecutive AA codes in Company B, e.g., AA01–AA05, then duplicate into Company B) | After 5 retries, `InvalidOperationException` thrown. Error message: "Unable to generate a unique code...". Transaction rolled back. No partial data. | | |
| R-03 | Name collision resolution | Duplicate a vendor whose name already exists in Company B | New vendor name = original + " (Copy)". Second duplicate = " (Copy 2)". Third = " (Copy 3)". | | |
| R-04 | Source vendor not found | Manually delete a vendor from DB between clicking Duplicate and confirming | "Vendor not found in your current company" message. No DB changes. | | |
| R-05 | Network timeout | (If testable) Kill connection mid-transaction | Transaction rolls back via `catch` block. Connection disposed via `using`. No connection leak. | | |

---

## G. Same-Tenant Validation

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| ST-01 | Same company blocked (vendor) | 1. Click Duplicate on a vendor 2. Select the CURRENT company (Company A) from dropdown 3. Confirm | Message: "Source and target companies must be different. Please select another company." No DB changes. | | |
| ST-02 | Same company blocked (customer) | Same as ST-01 for client | Same message. No DB changes. | | |
| ST-03 | Empty company selection | 1. Click Duplicate 2. Don't change dropdown (stays on "-- Select Target Company --") 3. Confirm | JS alert: "Please select a target company before duplicating." No postback. | | |

---

## H. Authorization / Access Control

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| AC-01 | No access to target company | Select a target company the user does NOT have access to (if testable) | `UnauthorizedAccessException` caught. Message: "You do not have permission to perform this action." | | |
| AC-02 | Session expired | Let session expire, then click Confirm | Redirected to `~/index.aspx`. | | |
| AC-03 | UserCanAccessCompany check | Verify the dropdown only shows companies the user has access to | `AuthGuard.GetAuthorizedCompanies()` populates dropdown. Current company excluded. | | |

---

## I. UI / Double-Submit Prevention

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| UI-01 | Double-submit vendor | Click "Confirm Duplicate" rapidly (twice) | Button disabled after first click. Text changes to "Duplicating...". Only one postback fires. | | |
| UI-02 | Double-submit customer | Same as UI-01 for client | Same behavior. | | |
| UI-03 | Cancel clears state | 1. Click Duplicate 2. Select company 3. Click Cancel 4. Click Duplicate on a different vendor | Modal shows correct new Vendor_Id (not stale from previous). `hfPendingVendorId` cleared in `finally`. | | |
| UI-04 | Button recovery | After a failed duplication | Button re-enabled, text restored to "Confirm Duplicate". | | |

---

## J. Concurrent Duplication (Advanced)

| # | Test Case | Steps | Expected | Evidence | Result |
|---|-----------|-------|----------|----------|--------|
| CC-01 | Two users duplicate to same target simultaneously | 1. User A and User B both click Duplicate on different vendors 2. Both target Company B 3. Both confirm at roughly the same time | Both transactions complete. Both get unique Vendor_Id codes (retry loop handles collision). Both audit entries written. | | |
| CC-02 | Same vendor duplicated twice rapidly | Click Duplicate on same vendor, confirm, then immediately do it again | Second duplication succeeds with a new code and "(Copy)" name suffix. Both records exist. | | |

---

## Evidence Collection Queries

### Pre-test snapshot
```sql
-- Run before testing begins
SELECT 'Before' AS Phase, 'tbl_Vendor' AS Table, COUNT(*) AS Rows, CompanyID
FROM tbl_Vendor WHERE CompanyID IN (<CompanyA>, <CompanyB>)
GROUP BY CompanyID;

SELECT 'Before' AS Phase, 'tbl_Client' AS Table, COUNT(*) AS Rows, CompanyID
FROM tbl_Client WHERE CompanyID IN (<CompanyA>, <CompanyB>)
GROUP BY CompanyID;

SELECT 'Before' AS Phase, COUNT(*) AS AuditRows
FROM tbl_SystemNotification
WHERE CompanyID IN (<CompanyA>, <CompanyB>) AND Title LIKE '%Duplicated%';
```

### Post-test snapshot
```sql
-- Run after each test case
SELECT 'After_V01' AS Test, Vendor_Id, Vendor_Name, CompanyID, CreatedBy, CreatedOn
FROM tbl_Vendor WHERE CompanyID = <CompanyB> ORDER BY Id DESC;

SELECT 'After_C01' AS Test, Client_Id, Client_Name, CompanyID, CreatedBy, CreatedOn
FROM tbl_Client WHERE CompanyID = <CompanyB> ORDER BY Id DESC;

SELECT TOP 5 'Audit' AS Test, CompanyID, Title, Message, UserId, CreatedOn
FROM tbl_SystemNotification
WHERE Title IN ('Vendor Duplicated', 'Customer Duplicated')
ORDER BY CreatedOn DESC;
```

### Child entity verification
```sql
-- After customer duplication
SELECT 'Address' AS Entity, COUNT(*) AS Rows
FROM tbl_ClientRegAddress WHERE Client_Id = '<new_AD>';

SELECT 'Factory' AS Entity, COUNT(*) AS Rows, CompanyID
FROM tbl_Factory WHERE Client_id = '<new_AD>';

SELECT 'Representative' AS Entity, COUNT(*) AS Rows, CompanyID
FROM tbl_representative WHERE Copany_Id = '<new_AD>';
```

---

## Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Tester | | | |
| Reviewer | | | |
| Approver | | | |
