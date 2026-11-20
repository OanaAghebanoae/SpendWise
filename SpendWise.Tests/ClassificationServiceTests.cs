using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.Models;
using SpendWise.Services;

namespace SpendWise.Tests;

public sealed class ClassificationServiceTests
{
    private static readonly Category Groceries = new() { Id = Guid.NewGuid(), Name = "Groceries" };
    private static readonly Category Travel = new() { Id = Guid.NewGuid(), Name = "Travel" };
    private static readonly Category Transfers = new() { Id = Guid.NewGuid(), Name = "Transfers" };

    private static CategoryRule Rule(RuleField field, RuleMatch match, string pattern, Category category, int priority = 100) =>
        new() { Id = Guid.NewGuid(), Field = field, Match = match, Pattern = pattern, Priority = priority, CategoryId = category.Id, Category = category };

    private static Transaction Tx(string operation, string? counterparty, string? description = null) =>
        new() { Id = Guid.NewGuid(), TransactionDate = new DateOnly(2026, 10, 1), Operation = operation, Counterparty = counterparty, Description = description ?? counterparty ?? operation, Amount = -10 };

    [Fact]
    public void ContainsMatchIsCaseInsensitive()
    {
        var rule = Rule(RuleField.Counterparty, RuleMatch.Contains, "lidl", Groceries);

        Assert.Same(rule, CategoryRuleMatcher.FindMatch(Tx("Cumparare POS", "LIDL RO 0103  RO  Iasi"), [rule]));
    }

    [Fact]
    public void EqualsDoesNotMatchPartialText()
    {
        var rule = Rule(RuleField.Description, RuleMatch.Equals, "Transfer", Transfers);

        Assert.Null(CategoryRuleMatcher.FindMatch(Tx("Transfer", "Some travel agency"), [rule]));
        Assert.Same(rule, CategoryRuleMatcher.FindMatch(Tx("Transfer", null, "Transfer"), [rule]));
    }

    [Fact]
    public void LowerPriorityWins()
    {
        var byCounterparty = Rule(RuleField.Counterparty, RuleMatch.Contains, "travel", Travel, priority: 10);
        var byOperation = Rule(RuleField.Operation, RuleMatch.Equals, "Transfer", Transfers, priority: 100);

        Assert.Same(byCounterparty, CategoryRuleMatcher.FindMatch(Tx("Transfer", "Escape travel srl"), [byOperation, byCounterparty]));
    }

    [Fact]
    public void MissingCounterpartyDoesNotMatch()
    {
        var rule = Rule(RuleField.Counterparty, RuleMatch.Contains, "LIDL", Groceries);

        Assert.Null(CategoryRuleMatcher.FindMatch(Tx("Transfer", null), [rule]));
    }

    [Fact]
    public async Task ApplySavesCategoryAndKeepsExistingOnes()
    {
        await using var db = new SpendWiseDbContext(new DbContextOptionsBuilder<SpendWiseDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var batch = new ImportBatch { Id = Guid.NewGuid(), FileName = "test.csv" };
        var lidl = Tx("Cumparare POS", "LIDL RO 0103");
        var unknown = Tx("Cumparare POS", "Unknown shop");
        var userChoice = Tx("Cumparare POS", "LIDL RO 0847");
        userChoice.CategoryId = Travel.Id;
        userChoice.ClassificationSource = "User";
        foreach (var t in new[] { lidl, unknown, userChoice }) { t.ImportBatch = batch; db.Transactions.Add(t); }
        db.Categories.AddRange(Groceries, Travel);
        db.CategoryRules.Add(new CategoryRule { Id = Guid.NewGuid(), Field = RuleField.Counterparty, Match = RuleMatch.Contains, Pattern = "LIDL", CategoryId = Groceries.Id });
        await db.SaveChangesAsync();

        var result = await new RuleBasedTransactionClassifier(db).ApplyAsync(CancellationToken.None);

        Assert.Equal(1, result.ClassifiedCount);
        Assert.Equal(1, result.StillUncategorizedCount);
        Assert.Equal(Groceries.Id, (await db.Transactions.FindAsync(lidl.Id))!.CategoryId);
        Assert.Equal(RuleBasedTransactionClassifier.Source, (await db.Transactions.FindAsync(lidl.Id))!.ClassificationSource);
        Assert.Null((await db.Transactions.FindAsync(unknown.Id))!.CategoryId);
        Assert.Equal(Travel.Id, (await db.Transactions.FindAsync(userChoice.Id))!.CategoryId);
    }
}
