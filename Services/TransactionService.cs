using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.DTOs;

namespace SpendWise.Services;

public sealed class TransactionService(SpendWiseDbContext db)
{
    public async Task<IReadOnlyList<TransactionDto>> SearchAsync(TransactionFilter filter, CancellationToken cancellationToken)
    {
        if (filter.From is not null && filter.To is not null && filter.From > filter.To) throw new ArgumentException("The start date must not be after the end date.");
        var query = db.Transactions.AsNoTracking().Include(x => x.Category).AsQueryable();
        if (filter.From is not null) query = query.Where(x => x.TransactionDate >= filter.From);
        if (filter.To is not null) query = query.Where(x => x.TransactionDate <= filter.To);
        if (filter.CategoryId is not null) query = query.Where(x => x.CategoryId == filter.CategoryId);
        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(x => x.Description.Contains(filter.Search) || (x.Counterparty != null && x.Counterparty.Contains(filter.Search)));
        return await query.OrderByDescending(x => x.TransactionDate).Select(x => new TransactionDto(x.Id, x.TransactionDate, x.Operation, x.Description, x.Amount, x.Balance, x.Category == null ? null : x.Category.Name, x.ClassificationSource, x.ClassificationConfidence)).ToListAsync(cancellationToken);
    }

    public async Task<bool> AssignCategoryAsync(Guid id, Guid? categoryId, CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions.FindAsync([id], cancellationToken);
        if (transaction is null) return false;
        if (categoryId is not null && !await db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken)) throw new ArgumentException("Category was not found.");
        transaction.CategoryId = categoryId;
        transaction.ClassificationSource = categoryId is null ? "Unclassified" : "User";
        transaction.ClassificationConfidence = null;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
