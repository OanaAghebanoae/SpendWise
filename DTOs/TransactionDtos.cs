namespace SpendWise.DTOs;

public sealed record TransactionDto(Guid Id, DateOnly TransactionDate, string Operation, string Description, decimal Amount, decimal? Balance, string? Category, string ClassificationSource, decimal? ClassificationConfidence);
public sealed record CategoryDto(Guid Id, string Name, string? Description);
public sealed record CreateCategoryRequest(string Name, string? Description);
public sealed record UpdateCategoryRequest(Guid? CategoryId);
public sealed record TransactionFilter(DateOnly? From, DateOnly? To, Guid? CategoryId, string? Search);
public sealed record ImportResultDto(Guid BatchId, int ImportedCount, int SkippedCount, IReadOnlyList<string> Errors, int ClassifiedCount = 0);
public sealed record ClassificationApplyResultDto(int ClassifiedCount, int StillUncategorizedCount);
public sealed record MonthlyCategoryTotal(string Category, decimal Total, int TransactionCount);
public sealed record MonthlyReportDto(int Year, int Month, decimal Expenses, decimal Income, IReadOnlyList<MonthlyCategoryTotal> ByCategory);
public sealed record ClassificationProposalDto(Guid TransactionId, string ProposedCategory, decimal Confidence, string Explanation);
