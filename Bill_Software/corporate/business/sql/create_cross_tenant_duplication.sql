/* ============================================================================
   NAME:        create_cross_tenant_duplication
   WHEN:        2026-10-02
   WHY:         Cross-tenant Vendor/Customer duplication only left a free-text
                audit row in tbl_SystemNotification. Source and target records
                need a persistent, queryable relationship so each side can show
                its authorized counterpart (docs/47).
   WHAT:        Creates dbo.tbl_CrossTenantDuplication with PK, EntityType CHECK,
                FKs SourceCompanyID/TargetCompanyID -> dbo.tbl_Company(ID),
                unique UQ_CrossTenantDuplication_Pair and lookup indexes
                IX_CrossTenantDuplication_Source / IX_CrossTenantDuplication_Target.
                Record IDs are intentionally not FK-bound (polymorphic). No
                changes to tbl_Vendor / tbl_Client. Idempotent.
   ============================================================================ */

IF OBJECT_ID(N'dbo.tbl_CrossTenantDuplication', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_CrossTenantDuplication
    (
        Id                  INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_CrossTenantDuplication PRIMARY KEY,
        EntityType          VARCHAR(20) NOT NULL,     -- 'Vendor' | 'Customer'
        SourceCompanyID     INT NOT NULL,
        SourceRecordID      INT NOT NULL,             -- tbl_Vendor.Id / tbl_Client.Id
        SourceBusinessCode  VARCHAR(50) NOT NULL,     -- Vendor_Id / Client_Id snapshot
        TargetCompanyID     INT NOT NULL,
        TargetRecordID      INT NOT NULL,
        TargetBusinessCode  VARCHAR(50) NOT NULL,
        DuplicatedOn        DATETIME NOT NULL
            CONSTRAINT DF_CrossTenantDuplication_DuplicatedOn DEFAULT (GETDATE()),
        DuplicatedBy        VARCHAR(50) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = N'CK_CrossTenantDuplication_EntityType'
                 AND parent_object_id = OBJECT_ID(N'dbo.tbl_CrossTenantDuplication'))
    ALTER TABLE dbo.tbl_CrossTenantDuplication
        ADD CONSTRAINT CK_CrossTenantDuplication_EntityType
        CHECK (EntityType IN ('Vendor', 'Customer'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = N'FK_CrossTenantDuplication_SourceCompany'
                 AND parent_object_id = OBJECT_ID(N'dbo.tbl_CrossTenantDuplication'))
    ALTER TABLE dbo.tbl_CrossTenantDuplication
        ADD CONSTRAINT FK_CrossTenantDuplication_SourceCompany
        FOREIGN KEY (SourceCompanyID) REFERENCES dbo.tbl_Company (ID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = N'FK_CrossTenantDuplication_TargetCompany'
                 AND parent_object_id = OBJECT_ID(N'dbo.tbl_CrossTenantDuplication'))
    ALTER TABLE dbo.tbl_CrossTenantDuplication
        ADD CONSTRAINT FK_CrossTenantDuplication_TargetCompany
        FOREIGN KEY (TargetCompanyID) REFERENCES dbo.tbl_Company (ID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
               WHERE name = N'UQ_CrossTenantDuplication_Pair'
                 AND parent_object_id = OBJECT_ID(N'dbo.tbl_CrossTenantDuplication'))
    ALTER TABLE dbo.tbl_CrossTenantDuplication
        ADD CONSTRAINT UQ_CrossTenantDuplication_Pair
        UNIQUE (EntityType, SourceCompanyID, SourceRecordID, TargetCompanyID, TargetRecordID);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CrossTenantDuplication_Source'
                 AND object_id = OBJECT_ID(N'dbo.tbl_CrossTenantDuplication'))
    CREATE INDEX IX_CrossTenantDuplication_Source
        ON dbo.tbl_CrossTenantDuplication (EntityType, SourceCompanyID, SourceRecordID)
        INCLUDE (TargetCompanyID, TargetBusinessCode, DuplicatedOn);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CrossTenantDuplication_Target'
                 AND object_id = OBJECT_ID(N'dbo.tbl_CrossTenantDuplication'))
    CREATE INDEX IX_CrossTenantDuplication_Target
        ON dbo.tbl_CrossTenantDuplication (EntityType, TargetCompanyID, TargetRecordID)
        INCLUDE (SourceCompanyID, SourceBusinessCode, DuplicatedOn);
GO
