using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.DTOs;

namespace SpendWise.Services;

public interface ITransactionClassifier
{
    Task<IReadOnlyList<ClassificationProposalDto>> ProposeAsync(CancellationToken cancellationToken);
}

public sealed class MockTransactionClassifier(SpendWiseDbContext db) : ITransactionClassifier
{
    public async Task<IReadOnlyList<ClassificationProposalDto>> ProposeAsync(CancellationToken cancellationToken)
    {
        var transactions = await db.Transactions.AsNoTracking().Where(x => x.CategoryId == null).ToListAsync(cancellationToken);
        return transactions.Select(x =>
        {
            var text = $"{x.Description} {x.Counterparty}".ToUpperInvariant();
            var category = text.Contains("CARREFOUR") || text.Contains("PROFI") || text.Contains("KAUFLAND") ? "Groceries" : text.Contains("NETFLIX") || text.Contains("SPOTIFY") ? "Subscriptions" : text.Contains("FARMAC") ? "Healthcare" : "Other";
            return new ClassificationProposalDto(x.Id, category, category == "Other" ? 0.35m : 0.85m, "Deterministic local mock classification based on transaction text.");
        }).ToList();
    }
}
