/* ============================================================================
   NAME:        category_merger_live_verify
   WHEN:        2026-11-10
   WHY:         READ-ONLY flamex_live census for Category Merger Option C. UAT must not be assumed equal to LIVE.
   WHAT:        No schema or data change. SELECT catalog metadata and aggregates only.
   ============================================================================ */
/*
Run in SSMS as a login that can SELECT the business tables.
Section 9 needs VIEW DEFINITION. Status NO VIEW DEFINITION PERMISSION means
grant VIEW DEFINITION or use a DBA login. Do not treat that status as a missing object.
Do not paste credentials into this file. Redact secrets if a definition contains any
before returning results. Results are counts and names of masters, not documents.
Each batch stops unless DB_NAME() is flamex_live.
Results to File or Text if section 9 is clipped in the grid.
*/

/* 1. Database and time. Expected: one row, DatabaseCheck = PASS, DatabaseName = flamex_live.
   A wrong database returns STOP and ends the batch. */
IF DB_NAME() <> N'flamex_live'
BEGIN
  SELECT DB_NAME() AS DatabaseName, SYSDATETIME() AS ExecutedAt, N'STOP: not flamex_live' AS DatabaseCheck;
  RETURN;
END
SET NOCOUNT ON;
SELECT DB_NAME() AS DatabaseName, SYSDATETIME() AS ExecutedAt, N'PASS' AS DatabaseCheck;
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 2. DBA / VIEW DEFINITION probe. NULL = securable not visible. 0 = no permission. 1 = granted.
   Expected: one row. CanSelect = 0 blocks later data batches. NULL is not proof the table is absent.
   -- CanSelect = 0 means subsequent data batches may fail with permission errors.
    -- NULL means the securable may be missing or not visible to the caller.
    -- Resolve permissions before interpreting incomplete results. */
SELECT
  HAS_PERMS_BY_NAME(N'dbo.tbl_NewparentProduct', N'OBJECT', N'SELECT') AS CanSelectCategory,
  HAS_PERMS_BY_NAME(N'dbo.tbl_NewProduct', N'OBJECT', N'SELECT') AS CanSelectProduct,
  HAS_PERMS_BY_NAME(N'dbo.tbl_stock', N'OBJECT', N'SELECT') AS CanSelectStock,
  HAS_PERMS_BY_NAME(N'dbo.tbl_NewProduct', N'OBJECT', N'VIEW DEFINITION') AS CanViewDefProduct,
  HAS_PERMS_BY_NAME(DB_NAME(), N'DATABASE', N'VIEW DEFINITION') AS CanViewDefDatabase;
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 3. Object visibility. Expected: one row per name.
   VISIBLE = catalog row and SELECT allowed.
   NO SELECT PERMISSION = catalog row, SELECT denied. Not the same as missing.
   NOT VISIBLE = no catalog row and permission probe NULL. Missing or hidden. Do not record as proven absent.
   Maps: GRN, CompanyStore, category history, legacy product pair, stock adjustment. */
SELECT v.ObjectName,
  CONVERT(varchar(60), o.type_desc) AS ObjectType,
  CASE
    WHEN o.object_id IS NULL THEN N'NOT VISIBLE'
    WHEN HAS_PERMS_BY_NAME(QUOTENAME(N'dbo') + N'.' + QUOTENAME(v.ObjectName), N'OBJECT', N'SELECT') = 0 THEN N'NO SELECT PERMISSION'
    WHEN HAS_PERMS_BY_NAME(QUOTENAME(N'dbo') + N'.' + QUOTENAME(v.ObjectName), N'OBJECT', N'SELECT') = 1 THEN N'VISIBLE'
    ELSE N'PERMISSION UNKNOWN'
  END AS Visibility
FROM (VALUES
  (N'tbl_NewparentProduct'), (N'tbl_NewProduct'), (N'tbl_stock'), (N'tbl_StockAdjustments'),
  (N'tbl_RequisitionMain'), (N'tbl_RequisitionNew'), (N'tbl_PO_Header'), (N'tbl_PO_Items'),
  (N'tbl_PO_Amendment_Items'), (N'tbl_PO_Item_Cancellation'),
  (N'tbl_Purches'), (N'tbl_purches_details'), (N'tbl_Invoice'), (N'tbl_Invoice_details'),
  (N'tbl_Chalan'), (N'tbl_Challan_details'), (N'tbl_Quotaion_details'), (N'tbl_Proforma_Details'),
  (N'tbl_Service'), (N'Stores'), (N'tbl_Product'), (N'tbl_parentProduct'),
  (N'CompanyStore'), (N'vw_CategoryStoreStock_Safe'), (N'vw_StockReconciliation')
) v(ObjectName)
LEFT JOIN sys.objects o ON o.name = v.ObjectName AND SCHEMA_NAME(o.schema_id) = N'dbo';
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 3b. Name search for objects this assessment must not invent.
   Expected: zero rows, or only objects already named above. An empty result is not proof of absence
   when section 2 shows the caller cannot see metadata. */
SELECT SCHEMA_NAME(schema_id) AS SchemaName, name AS ObjectName, type_desc AS ObjectType
FROM sys.objects
WHERE name LIKE N'%GRN%' OR name LIKE N'%GoodsReceipt%' OR name LIKE N'%CompanyStore%'
   OR name LIKE N'%CategoryHist%' OR name LIKE N'%CategoryMerge%' OR name LIKE N'%Categor%Audit%';
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 4. Column census. Expected: one row per existing visible column. A name that does not appear
   was not visible on a visible table. It is not proof about a table section 3 marked NOT VISIBLE.
   Category snapshot: ProductOrServiceCat / ParentCategoryId on transaction tables.
   Effective date: Effect / ValidFrom / AsOf on the two masters.
   Stock: CompanyID and category columns. Product_catagory is not the category id. */
SELECT t.name AS TableName, c.name AS ColumnName, ty.name AS TypeName,
  c.max_length AS MaxLength, c.is_nullable AS IsNullable
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE SCHEMA_NAME(t.schema_id) = N'dbo'
  AND t.name IN (
    N'tbl_NewparentProduct', N'tbl_NewProduct', N'tbl_stock', N'tbl_StockAdjustments',
    N'tbl_RequisitionNew', N'tbl_PO_Items', N'tbl_PO_Amendment_Items',
    N'tbl_purches_details', N'tbl_Invoice_details', N'tbl_Challan_details',
    N'tbl_Quotaion_details', N'tbl_Proforma_Details', N'tbl_Service', N'Stores')
  AND (
    c.name IN (
      N'id', N'Id', N'ID', N'ProductID', N'ProductId', N'Product_id', N'Product_Id',
      N'parentId', N'ParentCategoryId', N'ProductOrServiceCat', N'Product_catagory',
      N'service_catagory', N'Type', N'CompanyID', N'DeleteMode',
      N'Quantity', N'Quantity_Num', N'ShippedToStoreId', N'ShippedToStoreName', N'StoreId',
      N'Applied', N'AppliedAt', N'Reason')
    OR c.name LIKE N'%Effect%' OR c.name LIKE N'%ValidFrom%' OR c.name LIKE N'%AsOf%')
ORDER BY t.name, c.column_id;
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 5. Indexes. Expected: PK on category id, PK on product Id, unique ProductID,
   stock PK on Id only, no unique (Product_id, ShippedToStoreId). LIVE may differ. */
SELECT t.name AS TableName, i.name AS IndexName, i.is_unique AS IsUnique, i.is_primary_key AS IsPk,
  c.name AS ColumnName, ic.key_ordinal AS KeyOrdinal, ic.is_included_column AS IsIncluded
FROM sys.tables t
JOIN sys.indexes i ON i.object_id = t.object_id
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE SCHEMA_NAME(t.schema_id) = N'dbo'
  AND t.name IN (N'tbl_NewparentProduct', N'tbl_NewProduct', N'tbl_stock', N'tbl_PO_Items', N'tbl_RequisitionNew')
  AND i.type > 0
ORDER BY t.name, i.name, ic.key_ordinal;
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 6. Foreign keys. Expected question: product.parentId and PR.ParentCategoryId reference category id
   with NO_ACTION; PO ParentCategoryId has no FK. Delete of a used category is blocked by any FK shown.
   No row for a child table means no visible FK, not that the table is absent. */
SELECT fk.name AS FkName,
  OBJECT_NAME(fk.parent_object_id) AS ChildTable, pc.name AS ChildColumn,
  OBJECT_NAME(fk.referenced_object_id) AS ParentTable, rc.name AS ParentColumn,
  fk.delete_referential_action_desc AS OnDelete
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
JOIN sys.columns rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
WHERE OBJECT_NAME(fk.parent_object_id) IN (
    N'tbl_NewProduct', N'tbl_RequisitionNew', N'tbl_PO_Items', N'tbl_stock', N'tbl_StockAdjustments')
   OR OBJECT_NAME(fk.referenced_object_id) IN (N'tbl_NewparentProduct', N'tbl_NewProduct', N'Stores');
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 7. Triggers. Expected: names and disabled flag. Body is section 9.
   No row means no visible trigger on those object ids. OBJECT_ID NULL skips a hidden table. */
SELECT OBJECT_NAME(tr.parent_id) AS TableName, tr.name AS TriggerName, tr.is_disabled AS IsDisabled
FROM sys.triggers tr
WHERE tr.parent_id IN (
  OBJECT_ID(N'dbo.tbl_NewparentProduct'), OBJECT_ID(N'dbo.tbl_NewProduct'),
  OBJECT_ID(N'dbo.tbl_stock'), OBJECT_ID(N'dbo.tbl_purches_details'),
  OBJECT_ID(N'dbo.tbl_Invoice_details'), OBJECT_ID(N'dbo.tbl_PO_Items'),
  OBJECT_ID(N'dbo.tbl_RequisitionNew'));
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 8. Procedure and view names. Not a body. Empty is not proof of absence when section 2 cannot view definitions. */
SELECT SCHEMA_NAME(o.schema_id) AS SchemaName, o.name AS ObjectName, o.type_desc AS ObjectType
FROM sys.objects o
WHERE o.type IN (N'P', N'V', N'TR')
  AND (o.name LIKE N'%PO%' OR o.name LIKE N'%Requisition%' OR o.name LIKE N'%Product%'
    OR o.name LIKE N'%Stock%' OR o.name LIKE N'%Categor%' OR o.name LIKE N'%GRN%')
ORDER BY o.type_desc, o.name;
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 9. DBA PERMISSION. Requires VIEW DEFINITION. Status is MISSING only when the name is not in sys.objects.
   NO VIEW DEFINITION PERMISSION and DEFINITION NULL are permission or encryption results, not absence of the row.
   Expected: one row per name. Compare any PO insert with the repository script, which omits ParentCategoryId.
   sp_ReleasePO_Final in source only sets PO status. Check the trigger body for category or quantity writes. */
SELECT v.ObjectName,
  CONVERT(varchar(60), o.type_desc) AS ObjectType,
  CASE
    WHEN o.object_id IS NULL THEN N'NOT VISIBLE'
    WHEN HAS_PERMS_BY_NAME(QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(o.name), N'OBJECT', N'VIEW DEFINITION') = 0
      THEN N'NO VIEW DEFINITION PERMISSION'
    WHEN OBJECT_DEFINITION(o.object_id) IS NULL THEN N'DEFINITION NULL'
    ELSE N'OK'
  END AS DefinitionStatus,
  OBJECT_DEFINITION(o.object_id) AS DefinitionText
FROM (VALUES
  (N'sp_RequisitionItem_BulkUpsert'), (N'sp_ReleasePO_Final'),
  (N'sp_GetProductCategories'), (N'sp_SearchProductsFast'), (N'sp_GetProductStockByStore'),
  (N'vw_CategoryStoreStock_Safe'), (N'vw_StockReconciliation'), (N'trg_tbl_NewProduct_Normalize')
) v(ObjectName)
LEFT JOIN sys.objects o ON o.name = v.ObjectName AND o.schema_id = SCHEMA_ID(N'dbo');
GO
IF DB_NAME() <> N'flamex_live' RETURN;

/* 10. DBA PERMISSION. Modules whose visible text inserts PO lines or joins category from purchase, invoice, challan, or stock.
   Rows appear only when the definition text is visible. An empty result is inconclusive when section 9 is not OK.
   The following probe lists the same named modules with an explicit permission status and does not require the body. */
SELECT v.ObjectName,
  CASE
    WHEN o.object_id IS NULL THEN N'NOT VISIBLE'
    WHEN HAS_PERMS_BY_NAME(QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(o.name), N'OBJECT', N'VIEW DEFINITION') = 0
      THEN N'NO VIEW DEFINITION PERMISSION'
    WHEN OBJECT_DEFINITION(o.object_id) IS NULL THEN N'DEFINITION NULL'
    ELSE N'BODY VISIBLE'
  END AS DefinitionStatus
FROM (VALUES
  (N'sp_RequisitionItem_BulkUpsert'), (N'sp_ReleasePO_Final'),
  (N'sp_GetProductCategories'), (N'sp_SearchProductsFast'), (N'sp_GetProductStockByStore'),
  (N'vw_CategoryStoreStock_Safe'), (N'vw_StockReconciliation'), (N'trg_tbl_NewProduct_Normalize')
) v(ObjectName)
LEFT JOIN sys.objects o ON o.name = v.ObjectName AND o.schema_id = SCHEMA_ID(N'dbo');

SELECT OBJECT_SCHEMA_NAME(m.object_id) AS SchemaName, o.type_desc AS ObjectType, o.name AS ObjectName,
  CASE WHEN m.definition LIKE N'%tbl_PO_Items%' AND m.definition LIKE N'%INSERT%' THEN 1 ELSE 0 END AS MentionsPoInsert,
  CASE WHEN m.definition LIKE N'%ParentCategoryId%' THEN 1 ELSE 0 END AS MentionsParentCategoryId,
  CASE WHEN m.definition LIKE N'%ProductOrServiceCat%' THEN 1 ELSE 0 END AS MentionsCategoryName
FROM sys.sql_modules m
JOIN sys.objects o ON o.object_id = m.object_id
WHERE m.definition IS NOT NULL
  AND (
    (m.definition LIKE N'%tbl_PO_Items%' AND m.definition LIKE N'%INSERT%')
    OR (m.definition LIKE N'%tbl_purches_details%' AND (m.definition LIKE N'%tbl_NewProduct%' OR m.definition LIKE N'%ProductOrServiceCat%' OR m.definition LIKE N'%ParentCategoryId%'))
    OR (m.definition LIKE N'%tbl_Invoice_details%' AND (m.definition LIKE N'%tbl_NewProduct%' OR m.definition LIKE N'%ProductOrServiceCat%' OR m.definition LIKE N'%ParentCategoryId%'))
    OR (m.definition LIKE N'%tbl_Challan_details%' AND (m.definition LIKE N'%tbl_NewProduct%' OR m.definition LIKE N'%ProductOrServiceCat%' OR m.definition LIKE N'%ParentCategoryId%'))
    OR (m.definition LIKE N'%tbl_stock%' AND (m.definition LIKE N'%tbl_NewparentProduct%' OR m.definition LIKE N'%ProductOrServiceCat%' OR m.definition LIKE N'%ParentCategoryId%'))
  );
GO

/* 11. Aggregates. Each batch stops on the wrong database and fails only itself if that table or column is absent.
   A compile error names the missing object. Do not treat counts as defects by comparing them to UAT. */
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.1 Category ownership. Expected: count by CompanyID. */
SELECT CompanyID, COUNT(*) AS CategoryRows
FROM dbo.tbl_NewparentProduct GROUP BY CompanyID ORDER BY CompanyID;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.2 Duplicate trimmed category name inside a company. Expected: zero or more groups.
   nvarchar(max) keeps the full name. No 400-character cut. */
SELECT CompanyID, LTRIM(RTRIM(CONVERT(nvarchar(max), ProductOrServiceCat))) AS CategoryName, COUNT(*) AS NameRows
FROM dbo.tbl_NewparentProduct
GROUP BY CompanyID, LTRIM(RTRIM(CONVERT(nvarchar(max), ProductOrServiceCat)))
HAVING COUNT(*) > 1;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.3 Product ownership and type. Expected: count by CompanyID and Type. */
SELECT CompanyID, ISNULL(Type, N'(null)') AS TypeName, COUNT(*) AS ProductRows,
  SUM(CASE WHEN ISNULL(DeleteMode, 0) = 1 THEN 1 ELSE 0 END) AS SoftDeleted
FROM dbo.tbl_NewProduct
GROUP BY CompanyID, ISNULL(Type, N'(null)')
ORDER BY CompanyID, TypeName;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.4 Product/category consistency, one row per product CompanyID.
   Both names are nvarchar(max) before compare, so a long value is not cut to 400. */
SELECT
  p.CompanyID,
  COUNT(*) AS Products,
  SUM(CASE WHEN p.parentId IS NULL THEN 1 ELSE 0 END) AS NullParent,
  SUM(CASE WHEN p.parentId IS NOT NULL AND c.id IS NULL THEN 1 ELSE 0 END) AS OrphanParent,
  SUM(CASE WHEN c.id IS NOT NULL AND ISNULL(CONVERT(nvarchar(max), p.ProductOrServiceCat), N'') <> ISNULL(CONVERT(nvarchar(max), c.ProductOrServiceCat), N'') THEN 1 ELSE 0 END) AS NameMismatch,
  SUM(CASE WHEN c.id IS NOT NULL AND ISNULL(DATALENGTH(CONVERT(nvarchar(max), p.ProductOrServiceCat)), -1) <> ISNULL(DATALENGTH(CONVERT(nvarchar(max), c.ProductOrServiceCat)), -1) THEN 1 ELSE 0 END) AS NameLengthMismatch,
  SUM(CASE WHEN c.id IS NOT NULL AND c.CompanyID <> p.CompanyID THEN 1 ELSE 0 END) AS CompanyMismatch
FROM dbo.tbl_NewProduct p
LEFT JOIN dbo.tbl_NewparentProduct c ON c.id = p.parentId
GROUP BY p.CompanyID
ORDER BY p.CompanyID;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.5 PR category id coverage by PR CompanyID. DiffersFromCurrent = stored id <> current parentId.
   ProductCompanyMismatch counts a matched ProductID owned by another company. */
SELECT r.CompanyID,
  COUNT(*) AS PrLines,
  SUM(CASE WHEN r.ParentCategoryId IS NULL THEN 1 ELSE 0 END) AS NullCategory,
  SUM(CASE WHEN p.ProductID IS NULL THEN 1 ELSE 0 END) AS MissingProduct,
  SUM(CASE WHEN p.ProductID IS NOT NULL AND r.ParentCategoryId IS NOT NULL AND r.ParentCategoryId <> p.parentId THEN 1 ELSE 0 END) AS DiffersFromCurrent,
  SUM(CASE WHEN p.ProductID IS NOT NULL AND p.CompanyID <> r.CompanyID THEN 1 ELSE 0 END) AS ProductCompanyMismatch
FROM dbo.tbl_RequisitionNew r
LEFT JOIN dbo.tbl_NewProduct p ON p.ProductID = r.ProductId
GROUP BY r.CompanyID
ORDER BY r.CompanyID;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.6 PO category id coverage by PO line CompanyID. Same shape as PR. No receipt quantity exists to sum. */
SELECT i.CompanyID,
  COUNT(*) AS PoLines,
  SUM(CASE WHEN i.ParentCategoryId IS NULL THEN 1 ELSE 0 END) AS NullCategory,
  SUM(CASE WHEN p.ProductID IS NULL THEN 1 ELSE 0 END) AS MissingProduct,
  SUM(CASE WHEN p.ProductID IS NOT NULL AND i.ParentCategoryId IS NOT NULL AND i.ParentCategoryId <> p.parentId THEN 1 ELSE 0 END) AS DiffersFromCurrent,
  SUM(CASE WHEN p.ProductID IS NOT NULL AND p.CompanyID <> i.CompanyID THEN 1 ELSE 0 END) AS ProductCompanyMismatch
FROM dbo.tbl_PO_Items i
LEFT JOIN dbo.tbl_NewProduct p ON p.ProductID = i.ProductId
GROUP BY i.CompanyID
ORDER BY i.CompanyID;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
SELECT COUNT(*) AS AmendItems FROM dbo.tbl_PO_Amendment_Items;
SELECT COUNT(*) AS CancelItems FROM dbo.tbl_PO_Item_Cancellation;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.7 Row counts. Category columns are judged by section 4, not by these counts.
   Purchase lines have no CompanyID in the reviewed schema, so that count stays one total.
   Invoice and challan lines are grouped by their CompanyID. */
SELECT COUNT(*) AS PurchaseLines FROM dbo.tbl_purches_details;
SELECT CompanyID, COUNT(*) AS InvoiceLines
FROM dbo.tbl_Invoice_details GROUP BY CompanyID ORDER BY CompanyID;
SELECT CompanyID, COUNT(*) AS ChallanLines
FROM dbo.tbl_Challan_details GROUP BY CompanyID ORDER BY CompanyID;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.8 Quotation and proforma name snapshots by CompanyID. Blank and drift use the full nvarchar(max) value. */
SELECT qd.CompanyID,
  COUNT(*) AS QuotationLines,
  SUM(CASE WHEN qd.ProductOrServiceCat IS NULL OR LTRIM(RTRIM(CONVERT(nvarchar(max), qd.ProductOrServiceCat))) = N'' THEN 1 ELSE 0 END) AS BlankCategory,
  SUM(CASE WHEN p.ProductID IS NOT NULL AND ISNULL(CONVERT(nvarchar(max), qd.ProductOrServiceCat), N'') <> ISNULL(CONVERT(nvarchar(max), p.ProductOrServiceCat), N'') THEN 1 ELSE 0 END) AS DiffersFromCurrent
FROM dbo.tbl_Quotaion_details qd
LEFT JOIN dbo.tbl_NewProduct p ON p.ProductID = qd.Product_Code AND p.CompanyID = qd.CompanyID
GROUP BY qd.CompanyID
ORDER BY qd.CompanyID;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
SELECT pd.CompanyID,
  COUNT(*) AS ProformaLines,
  SUM(CASE WHEN pd.ProductOrServiceCat IS NULL OR LTRIM(RTRIM(CONVERT(nvarchar(max), pd.ProductOrServiceCat))) = N'' THEN 1 ELSE 0 END) AS BlankCategory,
  SUM(CASE WHEN p.ProductID IS NOT NULL AND ISNULL(CONVERT(nvarchar(max), pd.ProductOrServiceCat), N'') <> ISNULL(CONVERT(nvarchar(max), p.ProductOrServiceCat), N'') THEN 1 ELSE 0 END) AS DiffersFromCurrent
FROM dbo.tbl_Proforma_Details pd
LEFT JOIN dbo.tbl_NewProduct p ON p.ProductID = pd.Product_id AND p.CompanyID = pd.CompanyID
GROUP BY pd.CompanyID
ORDER BY pd.CompanyID;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.9 Stock grain. tbl_stock has no CompanyID, so the total is not split by company.
   Duplicate groups are counted, not listed. */
SELECT COUNT(*) AS StockRows,
  SUM(CASE WHEN ShippedToStoreId IS NULL OR LTRIM(RTRIM(ShippedToStoreId)) = N'' THEN 1 ELSE 0 END) AS BlankStore,
  SUM(CASE WHEN Quantity_Num IS NULL THEN 1 ELSE 0 END) AS NullQtyNum
FROM dbo.tbl_stock;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
SELECT COUNT(*) AS DuplicateProductStoreGroups
FROM (
  SELECT Product_id, ISNULL(ShippedToStoreId, N'') AS StoreId
  FROM dbo.tbl_stock
  GROUP BY Product_id, ISNULL(ShippedToStoreId, N'')
  HAVING COUNT(*) > 1
) d;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.10 Stock adjustment table. No CompanyID on the reviewed table, so Applied stays one total. */
SELECT COUNT(*) AS AdjRows,
  SUM(CASE WHEN Applied = 1 THEN 1 ELSE 0 END) AS AppliedRows,
  SUM(CASE WHEN Applied = 0 OR Applied IS NULL THEN 1 ELSE 0 END) AS NotApplied
FROM dbo.tbl_StockAdjustments;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.11 Service and product rows by CompanyID, and whether they have stock rows.
   tbl_stock has no CompanyID, so stock hits follow the product company. */
SELECT p.CompanyID, ISNULL(p.Type, N'(null)') AS TypeName,
  COUNT(DISTINCT p.ProductID) AS Products,
  SUM(CASE WHEN s.Id IS NULL THEN 0 ELSE 1 END) AS StockRowHits
FROM dbo.tbl_NewProduct p
LEFT JOIN dbo.tbl_stock s ON s.Product_id = p.ProductID
GROUP BY p.CompanyID, ISNULL(p.Type, N'(null)')
ORDER BY p.CompanyID, TypeName;
GO
IF DB_NAME() <> N'flamex_live' RETURN;
/* 11.12 Separate service master by CompanyID. Not a child of tbl_NewparentProduct unless section 6 shows an FK.
   Distinct categories are grouped as nvarchar(max). COUNT(DISTINCT) cannot take nvarchar(max). */
SELECT g.CompanyID, SUM(g.RowCnt) AS ServiceRows, COUNT(*) AS DistinctServiceCategoryValues
FROM (
  SELECT CompanyID, CONVERT(nvarchar(max), service_catagory) AS ServiceCategory, COUNT(*) AS RowCnt
  FROM dbo.tbl_Service
  GROUP BY CompanyID, CONVERT(nvarchar(max), service_catagory)
) g
GROUP BY g.CompanyID
ORDER BY g.CompanyID;
GO

/* Checklist. Section -> outstanding question. No query writes data.
   1  Connected database is flamex_live, and the run has a timestamp.
   2  Caller can SELECT masters/stock and VIEW DEFINITION. NULL is not absence. 0 is denied.
   3  Object visibility for GRN, CompanyStore, category history, masters, and stock. NOT VISIBLE is not proven absence.
   4  Category, CompanyID, snapshot, quantity, store, and effective-date columns on visible tables.
   5  ProductID uniqueness and stock grain indexes.
   6  FKs that block category delete, and whether PO ParentCategoryId has an FK.
   7  Trigger names.
   8  Procedure and view inventory. Empty is not proof of absence.
   9  DBA VIEW DEFINITION: bodies for PR upsert, PO release, category/stock procedures, two views, normalize trigger.
   10 DBA: permission status plus whether visible PO insert text includes ParentCategoryId.
   11.1-11.4 Category and product ownership, duplicate names, parent consistency.
   11.5-11.6 PR and PO snapshot coverage versus current parentId.
   11.7 Purchase, invoice, challan volumes. Snapshot coverage is section 4.
   11.8 Quotation and proforma snapshot blanks and drift.
   11.9-11.10 Stock integrity and adjustment Applied flag.
   11.11-11.12 Service products with stock, and the separate tbl_Service master.
*/
