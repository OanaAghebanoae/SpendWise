USE [SpendWise];
GO

SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;

DECLARE @Groceries uniqueidentifier = '11111111-1111-1111-1111-111111111111';
DECLARE @Transport uniqueidentifier = '22222222-2222-2222-2222-222222222222';
DECLARE @Entertainment uniqueidentifier = '33333333-3333-3333-3333-333333333333';
DECLARE @Income uniqueidentifier = '44444444-4444-4444-4444-444444444444';
DECLARE @Batch uniqueidentifier = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Groceries)
	INSERT INTO Categories (Id, Name, Description) VALUES (@Groceries, 'Groceries', 'Food and household shopping');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Transport)
	INSERT INTO Categories (Id, Name, Description) VALUES (@Transport, 'Transport', 'Fuel and public transport');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Entertainment)
	INSERT INTO Categories (Id, Name, Description) VALUES (@Entertainment, 'Entertainment', 'Leisure and subscriptions');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Income)
	INSERT INTO Categories (Id, Name, Description) VALUES (@Income, 'Income', 'Salary and other income');

-- Remaining categories, matched by name so the script doesn't duplicate ones created elsewhere.
DECLARE @MoreCategories TABLE (Name nvarchar(100), Description nvarchar(400));
INSERT INTO @MoreCategories VALUES
	(N'Eating out',            N'Restaurants, cafés, food delivery, cakes'),
	(N'Health & beauty',       N'Pharmacies, clinics, labs, dentist, vet, gym, drugstore, perfume'),
	(N'Shopping',              N'Clothes, shoes, online shops, books, toys, deliveries'),
	(N'Home',                  N'Household goods, home & garden'),
	(N'Utilities & phone',     N'Phone, internet, electricity, gas'),
	(N'Travel',                N'Hotels, trips, travel agencies'),
	(N'Transfers',             N'Between own accounts, to/from people'),
	(N'Savings & investments', N'Brokerage, mutual funds'),
	(N'Cash',                  N'ATM withdrawals'),
	(N'Other',                 N'Anything that does not fit');

INSERT INTO Categories (Id, Name, Description)
SELECT NEWID(), c.Name, c.Description FROM @MoreCategories c
WHERE NOT EXISTS (SELECT 1 FROM Categories x WHERE x.Name = c.Name);

IF NOT EXISTS (SELECT 1 FROM ImportBatches WHERE Id = @Batch)
	INSERT INTO ImportBatches (Id, FileName, ImportedAt, ImportedCount, SkippedCount)
	VALUES (@Batch, 'mock-data.csv', SYSUTCDATETIME(), 5, 0);

IF NOT EXISTS (SELECT 1 FROM Transactions WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000001')
	INSERT INTO Transactions (Id, TransactionDate, Operation, Description, Amount, Balance, Reference, Counterparty, CardNumber, CategoryId, ClassificationSource, ClassificationConfidence, CreatedAt, ImportBatchId)
	VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000001', '2026-10-01', 'Salary', 'Monthly salary', 4500.00, 4500.00, 'MOCK-SALARY-001', 'Example Employer', NULL, @Income, 'Mock', 1.00, SYSUTCDATETIME(), @Batch);

IF NOT EXISTS (SELECT 1 FROM Transactions WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000002')
	INSERT INTO Transactions (Id, TransactionDate, Operation, Description, Amount, Balance, Reference, Counterparty, CardNumber, CategoryId, ClassificationSource, ClassificationConfidence, CreatedAt, ImportBatchId)
	VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000002', '2026-10-03', 'POS', 'Weekly groceries', -125.50, 4374.50, 'MOCK-GROCERY-001', 'Carrefour', '**** 7153', @Groceries, 'Mock', 0.95, SYSUTCDATETIME(), @Batch);

IF NOT EXISTS (SELECT 1 FROM Transactions WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000003')
	INSERT INTO Transactions (Id, TransactionDate, Operation, Description, Amount, Balance, Reference, Counterparty, CardNumber, CategoryId, ClassificationSource, ClassificationConfidence, CreatedAt, ImportBatchId)
	VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', '2026-10-08', 'POS', 'Fuel', -80.00, 4294.50, 'MOCK-TRANSPORT-001', 'OMV', '**** 7153', @Transport, 'Mock', 0.90, SYSUTCDATETIME(), @Batch);

IF NOT EXISTS (SELECT 1 FROM Transactions WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000004')
	INSERT INTO Transactions (Id, TransactionDate, Operation, Description, Amount, Balance, Reference, Counterparty, CardNumber, CategoryId, ClassificationSource, ClassificationConfidence, CreatedAt, ImportBatchId)
	VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000004', '2026-10-15', 'Online', 'Streaming subscription', -45.00, 4249.50, 'MOCK-ENTERTAINMENT-001', 'Streaming Service', NULL, @Entertainment, 'Mock', 0.80, SYSUTCDATETIME(), @Batch);

IF NOT EXISTS (SELECT 1 FROM Transactions WHERE Id = 'aaaaaaaa-aaaa-aaaa-aaaa-000000000005')
	INSERT INTO Transactions (Id, TransactionDate, Operation, Description, Amount, Balance, Reference, Counterparty, CardNumber, CategoryId, ClassificationSource, ClassificationConfidence, CreatedAt, ImportBatchId)
	VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000005', '2026-10-20', 'POS', 'Unclassified purchase', -60.00, 4189.50, 'MOCK-UNCLASSIFIED-001', 'Local Store', NULL, NULL, 'Unclassified', NULL, SYSUTCDATETIME(), @Batch);

COMMIT TRANSACTION;
GO

SELECT Name, Description FROM Categories ORDER BY Name;
SELECT TransactionDate, Description, Amount, CategoryId FROM Transactions WHERE ImportBatchId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' ORDER BY TransactionDate;
