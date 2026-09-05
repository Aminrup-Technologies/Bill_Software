-- =============================================================================
-- When: Before deploying invoice Primary Service snapshot writes and export.
-- Why:  Invoice Excel currently looks up tbl_QutPrimaryService live, so later
--       quotation edits or deletes change historical invoice exports.
-- What: Add nullable tbl_Invoice.PServiceName to store the create-time snapshot.
-- =============================================================================
-- DO NOT execute until reviewed. Application INSERTs depend on this column.

ALTER TABLE tbl_Invoice
ADD PServiceName NVARCHAR(MAX) NULL;
