IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [ActorUserId] int NULL,
        [ActorRole] nvarchar(max) NOT NULL,
        [ActionType] nvarchar(max) NOT NULL,
        [EntityName] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NOT NULL,
        [ActionDescription] nvarchar(max) NOT NULL,
        [Source] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Brands] (
        [Id] int NOT NULL IDENTITY,
        [BrandName] nvarchar(max) NOT NULL,
        [BrandDescription] nvarchar(max) NOT NULL,
        [BrandLogo] varbinary(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [CategoryName] nvarchar(max) NOT NULL,
        [CategoryDescription] nvarchar(max) NOT NULL,
        [ParentCategoryId] int NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Categories] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(max) NOT NULL,
        [AllowNegativeStock] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomerCompanies] (
        [Id] int NOT NULL IDENTITY,
        [CustomerCompanyName] nvarchar(max) NOT NULL,
        [CustomerCompanyAddress] nvarchar(max) NOT NULL,
        [CustomerCompanyMail] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_CustomerCompanies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [LicenseInfos] (
        [Id] int NOT NULL IDENTITY,
        [LicenseKey] nvarchar(max) NOT NULL,
        [LicenseType] int NOT NULL,
        [UserLimit] int NOT NULL,
        [ExpirationDate] datetime2 NULL,
        [IsBypassed] bit NOT NULL,
        [LastCheckDate] datetime2 NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_LicenseInfos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Modules] (
        [Id] int NOT NULL IDENTITY,
        [ModuleName] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Modules] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [ProductUnits] (
        [Id] int NOT NULL IDENTITY,
        [UnitName] nvarchar(max) NOT NULL,
        [UnitShortName] nvarchar(max) NOT NULL,
        [IsDecimal] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_ProductUnits] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [RoleName] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] int NOT NULL IDENTITY,
        [SupplierCompanyName] nvarchar(max) NOT NULL,
        [SupplierContactName] nvarchar(max) NOT NULL,
        [SupplierContactMail] nvarchar(max) NOT NULL,
        [SupplierAddress] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] int NOT NULL IDENTITY,
        [SettingKey] nvarchar(max) NOT NULL,
        [SettingValue] nvarchar(max) NOT NULL,
        [LogoAscii] nvarchar(max) NOT NULL,
        [AppVersion] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Titles] (
        [Id] int NOT NULL IDENTITY,
        [TitleName] nvarchar(max) NOT NULL,
        [CompanyId] int NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Titles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Titles_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Warehouses] (
        [Id] int NOT NULL IDENTITY,
        [WarehouseName] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [CompanyId] int NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Warehouses_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [CustomerCompanyId] int NOT NULL,
        [CustomerContactPersonName] nvarchar(max) NOT NULL,
        [CustomerContactPersonLastName] nvarchar(max) NOT NULL,
        [CustomerContactPersonMobilPhone] nvarchar(max) NOT NULL,
        [CustomerContactPersonMail] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Customers_CustomerCompanies_CustomerCompanyId] FOREIGN KEY ([CustomerCompanyId]) REFERENCES [CustomerCompanies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [CreatedByUserId] int NULL,
        [UserName] nvarchar(255) NOT NULL,
        [UserPassword] nvarchar(max) NOT NULL,
        [UserMail] nvarchar(255) NOT NULL,
        [UserLastName] nvarchar(max) NOT NULL,
        [UserRegNo] nvarchar(max) NOT NULL,
        [CompanyId] int NULL,
        [TitleId] int NULL,
        [UserPicture] varbinary(max) NULL,
        [AccessFailedCount] int NOT NULL,
        [LockoutEnd] datetime2 NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Users_Titles_TitleId] FOREIGN KEY ([TitleId]) REFERENCES [Titles] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [ProductLocations] (
        [Id] int NOT NULL IDENTITY,
        [WarehouseId] int NOT NULL,
        [LocationCode] nvarchar(max) NOT NULL,
        [LocationDescription] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_ProductLocations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductLocations_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Offers] (
        [Id] int NOT NULL IDENTITY,
        [OfferNumber] nvarchar(max) NOT NULL,
        [CustomerId] int NOT NULL,
        [OfferDate] datetime2 NOT NULL,
        [ValidUntil] datetime2 NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [RowVersion] varbinary(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Offers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Offers_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [UserApiKeys] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [KeyName] nvarchar(max) NOT NULL,
        [ApiKey] nvarchar(max) NOT NULL,
        [SecretKey] nvarchar(max) NOT NULL,
        [ExpiresDate] datetime2 NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_UserApiKeys] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserApiKeys_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [UserPermissions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ModuleId] int NOT NULL,
        [CanRead] bit NOT NULL,
        [CanWrite] bit NOT NULL,
        [CanDelete] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPermissions_Modules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [Modules] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserPermissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [ProductName] nvarchar(max) NOT NULL,
        [ProductCode] nvarchar(450) NOT NULL,
        [CategoryId] int NOT NULL,
        [BrandId] int NOT NULL,
        [UnitId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [LocationId] int NULL,
        [CurrentStock] decimal(18,2) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_ProductLocations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [ProductLocations] ([Id]),
        CONSTRAINT [FK_Products_ProductUnits_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [ProductUnits] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceNumber] nvarchar(max) NOT NULL,
        [OfferId] int NULL,
        [SellerCompanyId] int NOT NULL,
        [BuyerCompanyId] int NOT NULL,
        [PreparedByUserId] int NOT NULL,
        [ApprovedByUserId] int NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [TaxTotal] decimal(18,2) NOT NULL,
        [GrandTotal] decimal(18,2) NOT NULL,
        [EInvoiceUuid] uniqueidentifier NULL,
        [Status] int NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoices_Companies_SellerCompanyId] FOREIGN KEY ([SellerCompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Invoices_CustomerCompanies_BuyerCompanyId] FOREIGN KEY ([BuyerCompanyId]) REFERENCES [CustomerCompanies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Invoices_Offers_OfferId] FOREIGN KEY ([OfferId]) REFERENCES [Offers] ([Id]),
        CONSTRAINT [FK_Invoices_Users_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Invoices_Users_PreparedByUserId] FOREIGN KEY ([PreparedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [OfferItems] (
        [Id] int NOT NULL IDENTITY,
        [OfferId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountRate] decimal(18,2) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_OfferItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OfferItems_Offers_OfferId] FOREIGN KEY ([OfferId]) REFERENCES [Offers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OfferItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [PriceLists] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [PurchasePrice] decimal(18,2) NOT NULL,
        [SalePrice] decimal(18,2) NOT NULL,
        [DiscountRate] decimal(18,2) NOT NULL,
        [VatRate] int NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [IsActivePrice] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_PriceLists] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceLists_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [StockAlerts] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [MinStockLevel] decimal(18,2) NOT NULL,
        [MaxStockLevel] decimal(18,2) NULL,
        [IsNotificationSent] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_StockAlerts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockAlerts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [StockMovements] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [WarehouseId] int NOT NULL,
        [TargetWarehouseId] int NULL,
        [MovementType] tinyint NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [MovementDate] datetime2 NOT NULL,
        [DocumentNo] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [UserId] int NOT NULL,
        [SupplierId] int NULL,
        [CustomerId] int NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockMovements_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]),
        CONSTRAINT [FK_StockMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StockMovements_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]),
        CONSTRAINT [FK_StockMovements_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StockMovements_Warehouses_TargetWarehouseId] FOREIGN KEY ([TargetWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StockMovements_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE TABLE [InvoiceItems] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [VatRate] int NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        CONSTRAINT [PK_InvoiceItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_InvoiceItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_ParentCategoryId] ON [Categories] ([ParentCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customers_CustomerCompanyId] ON [Customers] ([CustomerCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InvoiceItems_InvoiceId] ON [InvoiceItems] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InvoiceItems_ProductId] ON [InvoiceItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_ApprovedByUserId] ON [Invoices] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_BuyerCompanyId] ON [Invoices] ([BuyerCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_OfferId] ON [Invoices] ([OfferId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_PreparedByUserId] ON [Invoices] ([PreparedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_SellerCompanyId] ON [Invoices] ([SellerCompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OfferItems_OfferId] ON [OfferItems] ([OfferId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OfferItems_ProductId] ON [OfferItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Offers_CustomerId] ON [Offers] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PriceLists_ProductId] ON [PriceLists] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProductLocations_WarehouseId] ON [ProductLocations] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_LocationId] ON [Products] ([LocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_ProductCode] ON [Products] ([ProductCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_UnitId] ON [Products] ([UnitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_WarehouseId] ON [Products] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockAlerts_ProductId] ON [StockAlerts] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockMovements_CustomerId] ON [StockMovements] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockMovements_ProductId] ON [StockMovements] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockMovements_SupplierId] ON [StockMovements] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockMovements_TargetWarehouseId] ON [StockMovements] ([TargetWarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockMovements_UserId] ON [StockMovements] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StockMovements_WarehouseId] ON [StockMovements] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Titles_CompanyId] ON [Titles] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserApiKeys_UserId] ON [UserApiKeys] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserPermissions_ModuleId] ON [UserPermissions] ([ModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserPermissions_UserId] ON [UserPermissions] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_CompanyId] ON [Users] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_TitleId] ON [Users] ([TitleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_UserMail] ON [Users] ([UserMail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Warehouses_CompanyId] ON [Warehouses] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260210140657_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260210140657_InitialCreate', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260211093518_UpdateUserModel'
)
BEGIN
    ALTER TABLE [Users] ADD [UserFirstName] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260211093518_UpdateUserModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260211093518_UpdateUserModel', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260211125326_UpdateProductIndexToComposite'
)
BEGIN
    DROP INDEX [IX_Products_ProductCode] ON [Products];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260211125326_UpdateProductIndexToComposite'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_ProductCode_WarehouseId] ON [Products] ([ProductCode], [WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260211125326_UpdateProductIndexToComposite'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260211125326_UpdateProductIndexToComposite', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Warehouses] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [UserPermissions] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [UserApiKeys] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Titles] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [SystemSettings] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [StockMovements] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [StockAlerts] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Roles] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [ProductUnits] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Products] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [ProductLocations] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [PriceLists] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Offers] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [OfferItems] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Modules] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [LicenseInfos] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Invoices] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [InvoiceItems] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Customers] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [CustomerCompanies] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Companies] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Categories] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [Brands] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [CreatedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216073929_AddCreateAndAuditLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260216073929_AddCreateAndAuditLog', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216081153_AddActorUserNameToAuditLogs'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [ActorUserName] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260216081153_AddActorUserNameToAuditLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260216081153_AddActorUserNameToAuditLogs', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217082704_AddSupplierToPriceList'
)
BEGIN
    DROP INDEX [IX_Products_ProductCode_WarehouseId] ON [Products];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217082704_AddSupplierToPriceList'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'WarehouseId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Products] ALTER COLUMN [WarehouseId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217082704_AddSupplierToPriceList'
)
BEGIN
    ALTER TABLE [PriceLists] ADD [SupplierId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217082704_AddSupplierToPriceList'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Products_ProductCode_WarehouseId] ON [Products] ([ProductCode], [WarehouseId]) WHERE [WarehouseId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217082704_AddSupplierToPriceList'
)
BEGIN
    CREATE INDEX [IX_PriceLists_SupplierId] ON [PriceLists] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217082704_AddSupplierToPriceList'
)
BEGIN
    ALTER TABLE [PriceLists] ADD CONSTRAINT [FK_PriceLists_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217082704_AddSupplierToPriceList'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217082704_AddSupplierToPriceList', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217115821_AddOfferRowVersionConfig'
)
BEGIN
    ALTER TABLE [Offers] DROP CONSTRAINT [FK_Offers_Customers_CustomerId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217115821_AddOfferRowVersionConfig'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Offers]') AND [c].[name] = N'RowVersion');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Offers] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Offers] DROP COLUMN [RowVersion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217115821_AddOfferRowVersionConfig'
)
BEGIN
    ALTER TABLE [Offers] ADD [RowVersion] rowversion NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217115821_AddOfferRowVersionConfig'
)
BEGIN
    ALTER TABLE [Offers] ADD CONSTRAINT [FK_Offers_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217115821_AddOfferRowVersionConfig'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217115821_AddOfferRowVersionConfig', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217133425_AddCurrencyToOffer'
)
BEGIN
    ALTER TABLE [Offers] ADD [Currency] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217133425_AddCurrencyToOffer'
)
BEGIN
    ALTER TABLE [OfferItems] ADD [Currency] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217133425_AddCurrencyToOffer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217133425_AddCurrencyToOffer', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217140730_AddProductImageUrl'
)
BEGIN
    ALTER TABLE [Products] ADD [ImageUrl] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217140730_AddProductImageUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217140730_AddProductImageUrl', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217144111_AddQuickActionsToUser'
)
BEGIN
    ALTER TABLE [Users] ADD [QuickActionsJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260217144111_AddQuickActionsToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260217144111_AddQuickActionsToUser', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218124759_AddPasswordResetFields'
)
BEGIN
    ALTER TABLE [Users] ADD [PasswordResetToken] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218124759_AddPasswordResetFields'
)
BEGIN
    ALTER TABLE [Users] ADD [PasswordResetTokenExpires] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218124759_AddPasswordResetFields'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SystemSettings]') AND [c].[name] = N'LogoAscii');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [SystemSettings] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [SystemSettings] ALTER COLUMN [LogoAscii] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218124759_AddPasswordResetFields'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SystemSettings]') AND [c].[name] = N'AppVersion');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [SystemSettings] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [SystemSettings] ALTER COLUMN [AppVersion] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218124759_AddPasswordResetFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218124759_AddPasswordResetFields', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218131609_AddIsFullToPermissions'
)
BEGIN
    ALTER TABLE [UserPermissions] ADD [IsFull] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218131609_AddIsFullToPermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218131609_AddIsFullToPermissions', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218162402_AddAuditLogDetails'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [NewValues] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218162402_AddAuditLogDetails'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [OldValues] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218162402_AddAuditLogDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218162402_AddAuditLogDetails', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218164001_AddEntityDisplayToAuditLog'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [EntityDisplay] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218164001_AddEntityDisplayToAuditLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218164001_AddEntityDisplayToAuditLog', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219060320_AddWaybillTrackingToStockMovement'
)
BEGIN
    ALTER TABLE [StockMovements] ADD [DocumentPath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219060320_AddWaybillTrackingToStockMovement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219060320_AddWaybillTrackingToStockMovement', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219090256_AddTitles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219090256_AddTitles', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219102635_AddTitlePermissions'
)
BEGIN
    CREATE TABLE [TitlePermissions] (
        [Id] int NOT NULL IDENTITY,
        [TitleId] int NOT NULL,
        [ModuleId] int NOT NULL,
        [CanRead] bit NOT NULL,
        [CanWrite] bit NOT NULL,
        [CanDelete] bit NOT NULL,
        [IsFull] bit NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        [CreatedByUserId] int NULL,
        CONSTRAINT [PK_TitlePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TitlePermissions_Modules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [Modules] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TitlePermissions_Titles_TitleId] FOREIGN KEY ([TitleId]) REFERENCES [Titles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219102635_AddTitlePermissions'
)
BEGIN
    CREATE INDEX [IX_TitlePermissions_ModuleId] ON [TitlePermissions] ([ModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219102635_AddTitlePermissions'
)
BEGIN
    CREATE INDEX [IX_TitlePermissions_TitleId] ON [TitlePermissions] ([TitleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219102635_AddTitlePermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219102635_AddTitlePermissions', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    ALTER TABLE [Products] ADD [IsPhysical] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    ALTER TABLE [Products] ADD [ShelfId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE TABLE [DispatchNotes] (
        [Id] int NOT NULL IDENTITY,
        [DispatchNo] nvarchar(max) NOT NULL,
        [DispatchDate] datetime2 NOT NULL,
        [CompanyId] int NOT NULL,
        [CustomerId] int NULL,
        [DelivererUserId] int NULL,
        [DelivererName] nvarchar(max) NOT NULL,
        [ReceiverUserId] int NULL,
        [ReceiverName] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Note] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        [CreatedByUserId] int NULL,
        CONSTRAINT [PK_DispatchNotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DispatchNotes_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DispatchNotes_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE TABLE [WarehouseShelves] (
        [Id] int NOT NULL IDENTITY,
        [WarehouseId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        [CreatedByUserId] int NULL,
        CONSTRAINT [PK_WarehouseShelves] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WarehouseShelves_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE TABLE [DispatchNoteItems] (
        [Id] int NOT NULL IDENTITY,
        [DispatchNoteId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitName] nvarchar(max) NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        [CreatedByUserId] int NULL,
        CONSTRAINT [PK_DispatchNoteItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DispatchNoteItems_DispatchNotes_DispatchNoteId] FOREIGN KEY ([DispatchNoteId]) REFERENCES [DispatchNotes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DispatchNoteItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE INDEX [IX_Products_ShelfId] ON [Products] ([ShelfId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE INDEX [IX_DispatchNoteItems_DispatchNoteId] ON [DispatchNoteItems] ([DispatchNoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE INDEX [IX_DispatchNoteItems_ProductId] ON [DispatchNoteItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE INDEX [IX_DispatchNotes_CompanyId] ON [DispatchNotes] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE INDEX [IX_DispatchNotes_CustomerId] ON [DispatchNotes] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    CREATE INDEX [IX_WarehouseShelves_WarehouseId] ON [WarehouseShelves] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_WarehouseShelves_ShelfId] FOREIGN KEY ([ShelfId]) REFERENCES [WarehouseShelves] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219111859_AddLogisticModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219111859_AddLogisticModule', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219125725_AddSystemCodeToProduct'
)
BEGIN
    ALTER TABLE [Products] ADD [SystemCode] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219125725_AddSystemCodeToProduct'
)
BEGIN
    ALTER TABLE [Invoices] ADD [AssignedDelivererUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219125725_AddSystemCodeToProduct'
)
BEGIN
    ALTER TABLE [Invoices] ADD [ReceiverName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219125725_AddSystemCodeToProduct'
)
BEGIN
    ALTER TABLE [InvoiceItems] ADD [IncludeInDispatch] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219125725_AddSystemCodeToProduct'
)
BEGIN
    CREATE INDEX [IX_Invoices_AssignedDelivererUserId] ON [Invoices] ([AssignedDelivererUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219125725_AddSystemCodeToProduct'
)
BEGIN
    ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_Users_AssignedDelivererUserId] FOREIGN KEY ([AssignedDelivererUserId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219125725_AddSystemCodeToProduct'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219125725_AddSystemCodeToProduct', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219131348_MakeApprovedUserIdNullable'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Invoices]') AND [c].[name] = N'ApprovedByUserId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Invoices] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [Invoices] ALTER COLUMN [ApprovedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260219131348_MakeApprovedUserIdNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260219131348_MakeApprovedUserIdNullable', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223100922_SyncModelChanges'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'SystemCode');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [Products] ALTER COLUMN [SystemCode] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223100922_SyncModelChanges'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'ImageUrl');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [Products] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223100922_SyncModelChanges'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'CategoryDescription');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [Categories] ALTER COLUMN [CategoryDescription] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223100922_SyncModelChanges'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Brands]') AND [c].[name] = N'BrandLogo');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Brands] DROP CONSTRAINT ' + @var8 + ';');
    ALTER TABLE [Brands] ALTER COLUMN [BrandLogo] varbinary(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223100922_SyncModelChanges'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Brands]') AND [c].[name] = N'BrandDescription');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Brands] DROP CONSTRAINT ' + @var9 + ';');
    ALTER TABLE [Brands] ALTER COLUMN [BrandDescription] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260223100922_SyncModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260223100922_SyncModelChanges', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224080213_CompanyAddTaxAndAddress'
)
BEGIN
    ALTER TABLE [Companies] ADD [Address] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224080213_CompanyAddTaxAndAddress'
)
BEGIN
    ALTER TABLE [Companies] ADD [TaxNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224080213_CompanyAddTaxAndAddress'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260224080213_CompanyAddTaxAndAddress', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224114011_AddSystemQueueTasks'
)
BEGIN
    CREATE TABLE [SystemQueueTasks] (
        [Id] int NOT NULL IDENTITY,
        [TaskType] nvarchar(max) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [RetryCount] int NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ProcessedDate] datetime2 NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        [CreatedByUserId] int NULL,
        CONSTRAINT [PK_SystemQueueTasks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260224114011_AddSystemQueueTasks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260224114011_AddSystemQueueTasks', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225165604_SyncInvoiceAssignedDate'
)
BEGIN
    ALTER TABLE [Invoices] ADD [WarehouseAssignedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225165604_SyncInvoiceAssignedDate'
)
BEGIN
    ALTER TABLE [Invoices] ADD [WarehouseCompletedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225165604_SyncInvoiceAssignedDate'
)
BEGIN
    ALTER TABLE [Invoices] ADD [WarehouseIncompleteDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225165604_SyncInvoiceAssignedDate'
)
BEGIN
    CREATE TABLE [InvoiceStatusLogs] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [UserId] int NULL,
        [Status] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [PreparedItemsCount] int NOT NULL,
        [LogDate] datetime2 NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UpdatedByUserId] int NULL,
        [CreatedByUserId] int NULL,
        CONSTRAINT [PK_InvoiceStatusLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceStatusLogs_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_InvoiceStatusLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225165604_SyncInvoiceAssignedDate'
)
BEGIN
    CREATE INDEX [IX_InvoiceStatusLogs_InvoiceId] ON [InvoiceStatusLogs] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225165604_SyncInvoiceAssignedDate'
)
BEGIN
    CREATE INDEX [IX_InvoiceStatusLogs_UserId] ON [InvoiceStatusLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225165604_SyncInvoiceAssignedDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260225165604_SyncInvoiceAssignedDate', N'10.0.3');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260225170222_EnsureStableDB'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260225170222_EnsureStableDB', N'10.0.3');
END;

COMMIT;
GO

