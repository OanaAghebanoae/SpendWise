using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;
using SpendWise.Services;

namespace SpendWise.Tests;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task GroupsMonthlyExpensesAndIncome()
    {
        await using var db = CreateDb();
        var groceries = new Category { Id = Guid.NewGuid(), Name = "Groceries" };
        db.Categories.Add(groceries);
        db.Transactions.AddRange(
            new Transaction { Id = Guid.NewGuid(), TransactionDate = new DateOnly(2026, 10, 1), Description = "Food", Operation = "POS", Amount = -25, Category = groceries, ImportBatch = new ImportBatch { Id = Guid.NewGuid(), FileName = "test.csv" } },
            new Transaction { Id = Guid.NewGuid(), TransactionDate = new DateOnly(2026, 10, 2), Description = "Salary", Operation = "Income", Amount = 100, ImportBatch = new ImportBatch { Id = Guid.NewGuid(), FileName = "test.csv" } });
        await db.SaveChangesAsync();

        var report = await new ReportService(db).GetMonthlyAsync(2026, 10, CancellationToken.None);

        Assert.Equal(25, report.Expenses);
        Assert.Equal(100, report.Income);
        Assert.Equal("Groceries", Assert.Single(report.ByCategory).Category);
    }

    private static SpendWiseDbContext CreateDb() => new(new DbContextOptionsBuilder<SpendWiseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
