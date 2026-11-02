namespace SpendWise.Models;

public sealed class ImportBatch
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTimeOffset ImportedAt { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
