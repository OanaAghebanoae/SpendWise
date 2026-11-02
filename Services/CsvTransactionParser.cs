using System.Globalization;
using System.Text;

namespace SpendWise.Services;

public sealed record ParsedTransaction(DateOnly Date, string Operation, string Description, decimal Amount, decimal? Balance, string? Reference, string? Counterparty, string? CardNumber);
public sealed record CsvParseResult(IReadOnlyList<ParsedTransaction> Transactions, IReadOnlyList<string> Errors);

public interface ICsvTransactionParser
{
    Task<CsvParseResult> ParseAsync(Stream stream, CancellationToken cancellationToken);
}

public sealed class IngCsvTransactionParser : ICsvTransactionParser
{
    private static readonly Dictionary<string, int> RomanianMonths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ianuarie"] = 1, ["februarie"] = 2, ["martie"] = 3, ["aprilie"] = 4,
        ["mai"] = 5, ["iunie"] = 6, ["iulie"] = 7, ["august"] = 8,
        ["septembrie"] = 9, ["octombrie"] = 10, ["noiembrie"] = 11, ["decembrie"] = 12
    };

    public async Task<CsvParseResult> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var transactions = new List<ParsedTransaction>();
        var errors = new List<string>();
        ParsedTransactionBuilder? current = null;
        var lineNumber = 0;

        await foreach (var record in ReadRecordsAsync(reader, cancellationToken))
        {
            lineNumber++;
            var fields = SplitRecord(record);
            if (fields.Count < 8)
                continue;

            if (TryParseDate(fields[0], out var date))
            {
                if (current is not null)
                    transactions.Add(current.Build());

                if (!TryParseAmount(fields[4], out var debit) || !TryParseAmount(fields[6], out var credit) || !TryParseAmount(fields[7], out var balance))
                {
                    errors.Add($"Record {lineNumber} has an invalid amount.");
                    current = null;
                    continue;
                }

                current = new ParsedTransactionBuilder(date, fields[3], debit != 0 ? -debit : credit, balance);
                continue;
            }

            if (current is null)
                continue;

            var detail = fields[3].Trim();
            if (detail.StartsWith("Tranzactie la:", StringComparison.OrdinalIgnoreCase)) current.Counterparty = detail["Tranzactie la:".Length..].Trim();
            else if (detail.StartsWith("Beneficiar:", StringComparison.OrdinalIgnoreCase) || detail.StartsWith("Ordonator:", StringComparison.OrdinalIgnoreCase)) current.Counterparty = detail[(detail.IndexOf(':') + 1)..].Trim();
            else if (detail.StartsWith("Referinta:", StringComparison.OrdinalIgnoreCase)) current.Reference = detail["Referinta:".Length..].Trim();
            else if (detail.StartsWith("Numar card:", StringComparison.OrdinalIgnoreCase)) current.CardNumber = detail["Numar card:".Length..].Trim();
        }

        if (current is not null)
            transactions.Add(current.Build());

        return new CsvParseResult(transactions, errors);
    }

    private static bool TryParseDate(string value, out DateOnly date)
    {
        var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3 && int.TryParse(parts[0], out var day) && int.TryParse(parts[2], out var year) && RomanianMonths.TryGetValue(parts[1], out var month))
        {
            date = new DateOnly(year, month, day);
            return true;
        }
        date = default;
        return false;
    }

    private static bool TryParseAmount(string value, out decimal amount)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            amount = 0;
            return true;
        }

        var normalized = value.Trim().Replace(".", string.Empty).Replace(",", ".");
        return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out amount);
    }

    private static List<string> SplitRecord(string record)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < record.Length; i++)
        {
            var c = record[i];
            if (c == '"')
            {
                if (quoted && i + 1 < record.Length && record[i + 1] == '"') { value.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (c == ',' && !quoted) { values.Add(value.ToString()); value.Clear(); }
            else value.Append(c);
        }
        values.Add(value.ToString());
        return values;
    }

    private static async IAsyncEnumerable<string> ReadRecordsAsync(StreamReader reader, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = new StringBuilder();
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (buffer.Length > 0) buffer.AppendLine();
            buffer.Append(line);
            if (CountQuotes(buffer.ToString()) % 2 == 0)
            {
                yield return buffer.ToString();
                buffer.Clear();
            }
        }
        if (buffer.Length > 0) yield return buffer.ToString();
    }

    private static int CountQuotes(string value) => value.Count(c => c == '"');

    private sealed class ParsedTransactionBuilder(DateOnly date, string operation, decimal amount, decimal? balance)
    {
        public string? Reference { get; set; }
        public string? Counterparty { get; set; }
        public string? CardNumber { get; set; }
        public ParsedTransaction Build() => new(date, operation.Trim(), Counterparty ?? operation.Trim(), amount, balance, Reference, Counterparty, CardNumber);
    }
}
