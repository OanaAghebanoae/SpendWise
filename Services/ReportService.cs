using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.DTOs;

namespace SpendWise.Services;

public sealed class ReportService(SpendWiseDbContext db)
{
    public async Task<MonthlyReportDto> GetMonthlyAsync(int year, int month, CancellationToken cancellationToken)
    {
        if (month is < 1 or > 12) throw new ArgumentException("Month must be between 1 and 12.");
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1);
        var transactions = await db.Transactions.AsNoTracking().Include(x => x.Category).Where(x => x.TransactionDate >= start && x.TransactionDate < end).ToListAsync(cancellationToken);
        var expenses = transactions.Where(x => x.Amount < 0).Sum(x => -x.Amount);
        var income = transactions.Where(x => x.Amount > 0).Sum(x => x.Amount);
        var byCategory = transactions.Where(x => x.Amount < 0).GroupBy(x => x.Category?.Name ?? "Unclassified").OrderBy(x => x.Key).Select(x => new MonthlyCategoryTotal(x.Key, x.Sum(t => -t.Amount), x.Count())).ToList();
        return new MonthlyReportDto(year, month, expenses, income, byCategory);
    }
}
