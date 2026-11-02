namespace SpendWise.Models;

public sealed class Transaction
{
    public Guid Id { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? Balance { get; set; }
    public string? Reference { get; set; }
    public string? Counterparty { get; set; }
    public string? CardNumber { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string ClassificationSource { get; set; } = "Unclassified";
    public decimal? ClassificationConfidence { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid ImportBatchId { get; set; }
    public ImportBatch ImportBatch { get; set; } = null!;
}
