# SpendWise

SpendWise is a .NET 10 Web API for importing banking transactions, assigning categories, and generating monthly spending reports.

## Local database

The API uses SQL Server with the `ApplicationManagementDatabase` connection string. The default development value is:

```text
Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SpendWise;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True
```

You can override it without changing source code:

```powershell
$env:ConnectionStrings__ApplicationManagementDatabase = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SpendWise;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True"
```

Create or update the schema from the repository root:

```powershell
dotnet ef database update --project SpendWise.csproj
```

The API does not automatically create, drop, or migrate the database at startup. This avoids destructive or unexpected schema changes.

## Run the API

```powershell
dotnet run --project SpendWise.csproj
```

OpenAPI is available in development at `/openapi/v1.json`.

## CSV import contract

The initial importer supports the ING Romania export format represented by `Tranzactii_30-10-2026_11-17-19.csv`:

- comma-delimited, eight-column rows;
- Romanian month names such as `30 octombrie 2026`;
- debit in column 5 and credit in column 7, with comma decimal separators;
- balance in column 8;
- transaction details in following rows, including merchant, counterparty, card, reference, and authorization information;
- account-holder metadata, signatures, blank rows, and repeated headers are ignored.

Only rows with a valid date in the first column are transaction records. Debit values are stored as negative amounts and credit values as positive amounts.

## Tests

Run all tests with:

```powershell
dotnet test
```
