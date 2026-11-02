using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;
using SpendWise.Services;

namespace SpendWise.Tests;

public sealed class TransactionServiceTests
{
    [Fact]
    public async Task RejectsInvalidDateRange()
    {
        await using var db = new SpendWiseDbContext(new DbContextOptionsBuilder<SpendWiseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        await Assert.ThrowsAsync<ArgumentException>(() => new TransactionService(db).SearchAsync(new(null, new DateOnly(2026, 1, 1), null, null), CancellationToken.None));
    }
}
