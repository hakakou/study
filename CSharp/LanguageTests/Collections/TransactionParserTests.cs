using System;
using System.Collections.Generic;
using Xunit;

namespace LanguageTests.Collections;

public enum TxnStatus
{
    Unknown,
    Approved,
    Declined,
    Pending,
    Reversed
}

public readonly record struct TxnSummary(decimal TotalAmount, int Count);

public static class TransactionParser
{
    // TODO: Implement this method using ReadOnlySpan<char>.
    // It must:
    //   - Take a single log line (pipe-delimited, 6 fields)
    //   - Return (amount, status) without allocating any intermediate strings
    //   - Throw FormatException if the line is malformed
    public static (decimal amount, Collections.TxnStatus status) ParseLine(string line)
    {
        throw new NotImplementedException();
    }

    // TODO: Implement this method.
    // It must aggregate totals per status across many lines,
    // using ParseLine internally. No LINQ, no Split, no Substring.
    public static IReadOnlyDictionary<Collections.TxnStatus, Collections.TxnSummary> Aggregate(IEnumerable<string> lines)
    {
        throw new NotImplementedException();
    }
}

public class TransactionParserTests
{
    [Theory]
    [InlineData("2025-03-14T08:42:11|TXN-8842719|USD|1250.75|MERCHANT-2204|APPROVED", 1250.75, Collections.TxnStatus.Approved)]
    [InlineData("2025-03-14T08:42:12|TXN-8842720|EUR|42.00|MERCHANT-0001|DECLINED", 42.00, Collections.TxnStatus.Declined)]
    [InlineData("2025-03-14T08:42:13|TXN-8842721|GBP|999999.99|MERCHANT-9999|PENDING", 999999.99, Collections.TxnStatus.Pending)]
    [InlineData("2025-03-14T08:42:14|TXN-8842722|USD|0.01|MERCHANT-0002|REVERSED", 0.01, Collections.TxnStatus.Reversed)]
    public void ParseLine_ValidInput_ReturnsExpected(string line, double expectedAmount, Collections.TxnStatus expectedStatus)
    {
        var (amount, status) = Collections.TransactionParser.ParseLine(line);
        Assert.Equal((decimal)expectedAmount, amount);
        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void Aggregate_MultipleLines_SumsPerStatus()
    {
        var lines = new[]
        {
            "2025-03-14T08:42:11|TXN-001|USD|100.00|M-1|APPROVED",
            "2025-03-14T08:42:12|TXN-002|USD|250.50|M-1|APPROVED",
            "2025-03-14T08:42:13|TXN-003|USD|75.25|M-2|DECLINED",
            "2025-03-14T08:42:14|TXN-004|USD|10.00|M-2|APPROVED",
            "2025-03-14T08:42:15|TXN-005|USD|500.00|M-3|PENDING",
        };

        var result = Collections.TransactionParser.Aggregate(lines);

        Assert.Equal(new Collections.TxnSummary(360.50m, 3), result[Collections.TxnStatus.Approved]);
        Assert.Equal(new Collections.TxnSummary(75.25m, 1), result[Collections.TxnStatus.Declined]);
        Assert.Equal(new Collections.TxnSummary(500.00m, 1), result[Collections.TxnStatus.Pending]);
    }

    [Fact]
    public void ParseLine_MalformedLine_Throws()
    {
        Assert.Throws<FormatException>(() =>
            Collections.TransactionParser.ParseLine("2025-03-14T08:42:11|TXN-001|USD|100.00"));
    }

    // ⚠️ TRICK QUESTION — read carefully before implementing.
    // This test must pass. Think about WHY it might fail with a naive implementation.
    [Fact]
    public void ParseLine_StatusFieldIsLastWithNoTrailingDelimiter_ParsesCorrectly()
    {
        var line = "2025-03-14T08:42:11|TXN-001|USD|42.42|M-1|APPROVED";
        var (amount, status) = Collections.TransactionParser.ParseLine(line);

        Assert.Equal(42.42m, amount);
        Assert.Equal(Collections.TxnStatus.Approved, status);
    }
}