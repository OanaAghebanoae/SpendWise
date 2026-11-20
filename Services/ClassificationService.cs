using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.DTOs;
using SpendWise.Models;

namespace SpendWise.Services;

public interface ITransactionClassifier
{
    /// <summary>Suggests a category for every uncategorized transaction, without saving anything.</summary>
    Task<IReadOnlyList<ClassificationProposalDto>> ProposeAsync(CancellationToken cancellationToken);

    /// <summary>Saves a category on every uncategorized transaction a rule matches. Never overrides an existing category.</summary>
    Task<ClassificationApplyResultDto> ApplyAsync(CancellationToken cancellationToken);
}

/// <summary>Classifies transactions using the rules stored in the CategoryRules table.</summary>
public sealed class RuleBasedTransactionClassifier(SpendWiseDbContext db) : ITransactionClassifier
{
    public const string Source = "Rule";

    public async Task<IReadOnlyList<ClassificationProposalDto>> ProposeAsync(CancellationToken cancellationToken)
    {
        var rules = await LoadRulesAsync(cancellationToken);
        var transactions = await db.Transactions.AsNoTracking().Where(x => x.CategoryId == null).ToListAsync(cancellationToken);

        return transactions
            .Select(x => (Transaction: x, Rule: CategoryRuleMatcher.FindMatch(x, rules)))
            .Where(x => x.Rule is not null)
            .Select(x => new ClassificationProposalDto(x.Transaction.Id, x.Rule!.Category.Name, 1.0m, CategoryRuleMatcher.Explain(x.Rule)))
            .ToList();
    }

    public async Task<ClassificationApplyResultDto> ApplyAsync(CancellationToken cancellationToken)
    {
        var rules = await LoadRulesAsync(cancellationToken);
        var transactions = await db.Transactions.Where(x => x.CategoryId == null).ToListAsync(cancellationToken);

        var classified = 0;
        foreach (var transaction in transactions)
        {
            var rule = CategoryRuleMatcher.FindMatch(transaction, rules);
            if (rule is null) continue;
            transaction.CategoryId = rule.CategoryId;
            transaction.ClassificationSource = Source;
            transaction.ClassificationConfidence = 1.0m;
            classified++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ClassificationApplyResultDto(classified, transactions.Count - classified);
    }

    private async Task<IReadOnlyList<CategoryRule>> LoadRulesAsync(CancellationToken cancellationToken) =>
        await db.CategoryRules.AsNoTracking().Include(x => x.Category).ToListAsync(cancellationToken);
}

public static class CategoryRuleMatcher
{
    /// <summary>
    /// Returns the winning rule for a transaction, or null. Lowest Priority wins;
    /// on a tie the longer (more specific) pattern wins.
    /// </summary>
    public static CategoryRule? FindMatch(Transaction transaction, IEnumerable<CategoryRule> rules) =>
        rules
            .Where(rule => Matches(transaction, rule))
            .OrderBy(rule => rule.Priority)
            .ThenByDescending(rule => rule.Pattern.Length)
            .FirstOrDefault();

    public static bool Matches(Transaction transaction, CategoryRule rule)
    {
        var value = rule.Field switch
        {
            RuleField.Counterparty => transaction.Counterparty,
            RuleField.Operation => transaction.Operation,
            RuleField.Description => transaction.Description,
            _ => null
        };
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(rule.Pattern)) return false;

        return rule.Match switch
        {
            RuleMatch.Contains => value.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
            RuleMatch.Equals => string.Equals(value.Trim(), rule.Pattern.Trim(), StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    public static string Explain(CategoryRule rule) =>
        $"{rule.Field} {rule.Match.ToString().ToLowerInvariant()} '{rule.Pattern}'";
}
