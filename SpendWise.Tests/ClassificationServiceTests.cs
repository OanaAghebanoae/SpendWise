using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;
using SpendWise.Services;

namespace SpendWise.Tests;

public sealed class ClassificationServiceTests
{
    [Fact]
    public async Task ProposesGroceriesForKnownMerchant()
    {
        await using var db = new SpendWiseDbContext(new DbContextOptionsBuilder<SpendWiseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Transactions.Add(new Transaction { Id = Guid.NewGuid(), TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow), Description = "POS", Counterparty = "Carrefour", Amount = -10, ImportBatch = new ImportBatch { Id = Guid.NewGuid(), FileName = "test.csv" } });
        await db.SaveChangesAsync();

        var proposal = Assert.Single(await new MockTransactionClassifier(db).ProposeAsync(CancellationToken.None));

        Assert.Equal("Groceries", proposal.ProposedCategory);
        Assert.True(proposal.Confidence > 0);
    }
}
