using System.Text;
using SpendWise.Services;

namespace SpendWise.Tests;

public sealed class CsvTransactionParserTests
{
    [Fact]
    public async Task ParsesRomanianDateDebitAndMerchant()
    {
        const string csv = "Data,,,Detalii tranzactie,Debit,,Credit,Balanta\n30 octombrie 2026,,,Cumparare POS,\"36,95\",,,\"177,29\"\n,,,Tranzactie la:CARREFOUR MK PALAS C2  RO  IASI,,,,\n,,,Numar card:**** 7153,,,,";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var result = await new IngCsvTransactionParser().ParseAsync(stream, CancellationToken.None);

        var transaction = Assert.Single(result.Transactions);
        Assert.Equal(new DateOnly(2026, 10, 30), transaction.Date);
        Assert.Equal(-36.95m, transaction.Amount);
        Assert.Equal("CARREFOUR MK PALAS C2  RO  IASI", transaction.Counterparty);
        Assert.Empty(result.Errors);
    }
}
