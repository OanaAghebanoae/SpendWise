using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.DTOs;
using SpendWise.Models;

namespace SpendWise.Services;

public sealed class ImportService(SpendWiseDbContext db, ICsvTransactionParser parser, ITransactionClassifier classifier)
{
    public async Task<ImportResultDto> ImportAsync(string fileName, Stream stream, CancellationToken cancellationToken)
    {
        var parsed = await parser.ParseAsync(stream, cancellationToken);
        var batch = new ImportBatch { Id = Guid.NewGuid(), FileName = Path.GetFileName(fileName), ImportedAt = DateTimeOffset.UtcNow };
        foreach (var item in parsed.Transactions)
        {
            var duplicate = await db.Transactions.AnyAsync(x => x.TransactionDate == item.Date && x.Amount == item.Amount && x.Description == item.Description && x.Reference == item.Reference, cancellationToken);
            if (duplicate) { batch.SkippedCount++; continue; }
            batch.Transactions.Add(new Transaction { Id = Guid.NewGuid(), TransactionDate = item.Date, Operation = item.Operation, Description = item.Description, Amount = item.Amount, Balance = item.Balance, Reference = item.Reference, Counterparty = item.Counterparty, CardNumber = item.CardNumber, CreatedAt = DateTimeOffset.UtcNow });
            batch.ImportedCount++;
        }
        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);

        // Categorize the new rows straight away using the stored rules.
        var classification = await classifier.ApplyAsync(cancellationToken);
        return new ImportResultDto(batch.Id, batch.ImportedCount, batch.SkippedCount, parsed.Errors, classification.ClassifiedCount);
    }
}
