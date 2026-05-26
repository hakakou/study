using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace LanguageTests.Memory;

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
    public static (decimal amount, Memory.TxnStatus status) ParseLine(string line)
    {
        ReadOnlySpan<char> span = line.AsSpan();
        Span<char> vowels = ['a', 'e', 'i', 'o', 'u'];

        decimal amount = 0;
        for (int p = 0; p < 6; p++)
        {
            int i = span.IndexOf('|');
            var field = i < 0 ? span : span[0..i];

            if (p == 3 && !decimal.TryParse(field, CultureInfo.InvariantCulture, out amount))
                throw new FormatException();

            if (p == 5)
            {
                if (!Enum.TryParse<Memory.TxnStatus>(field, true, out var t))
                    throw new FormatException();

                // MemoryExtensions.Equals(field, "APPROVED", StringComparison.Ordinal)

                return (amount, t);
            }

            if (i < 0) break;
            span = span[(i + 1)..];
        }
        throw new FormatException();


        /* Better way
        Span<Range> ranges = stackalloc Range[6];
        var cols = span.Split(ranges, '|', StringSplitOptions.None);
        if (cols != 6)
            throw new FormatException("Cannot parse");

        var amount = decimal.Parse(span[ranges[3]], CultureInfo.InvariantCulture);
        var status = Enum.Parse<TxnStatus>(span[ranges[5]], true);
        return (amount.Value, status.Value);
        */
    }

    // TODO: Implement this method.
    // It must aggregate totals per status across many lines,
    // using ParseLine internally. No LINQ, no Split, no Substring.
    public static IReadOnlyDictionary<Memory.TxnStatus, Memory.TxnSummary> Aggregate(IEnumerable<string> lines)
    {
        var dic = new Dictionary<Memory.TxnStatus, Memory.TxnSummary>(lines.Count());
        foreach (var line in lines)
        {
            var (amount, status) = ParseLine(line);

            ref var s = ref CollectionsMarshal.GetValueRefOrAddDefault(dic, status, out _);
            s = new Memory.TxnSummary(s.TotalAmount + amount, s.Count + 1);
        }
        return dic;
    }
}

public class TransactionParserTests
{
    [Theory]
    [InlineData("2025-03-14T08:42:11|TXN-8842719|USD|1250.75|MERCHANT-2204|APPROVED", 1250.75, Memory.TxnStatus.Approved)]
    [InlineData("2025-03-14T08:42:12|TXN-8842720|EUR|42.00|MERCHANT-0001|DECLINED", 42.00, Memory.TxnStatus.Declined)]
    [InlineData("2025-03-14T08:42:13|TXN-8842721|GBP|999999.99|MERCHANT-9999|PENDING", 999999.99, Memory.TxnStatus.Pending)]
    [InlineData("2025-03-14T08:42:14|TXN-8842722|USD|0.01|MERCHANT-0002|REVERSED", 0.01, Memory.TxnStatus.Reversed)]
    public void ParseLine_ValidInput_ReturnsExpected(string line, double expectedAmount, Memory.TxnStatus expectedStatus)
    {
        var (amount, status) = Memory.TransactionParser.ParseLine(line);
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

        var result = Memory.TransactionParser.Aggregate(lines);

        Assert.Equal(new Memory.TxnSummary(360.50m, 3), result[Memory.TxnStatus.Approved]);
        Assert.Equal(new Memory.TxnSummary(75.25m, 1), result[Memory.TxnStatus.Declined]);
        Assert.Equal(new Memory.TxnSummary(500.00m, 1), result[Memory.TxnStatus.Pending]);
    }

    [Fact]
    public void ParseLine_MalformedLine_Throws()
    {
        Assert.Throws<FormatException>(() =>
            Memory.TransactionParser.ParseLine("2025-03-14T08:42:11|TXN-001|USD|100.00"));
    }

    // ⚠️ TRICK QUESTION — read carefully before implementing.
    // This test must pass. Think about WHY it might fail with a naive implementation.
    [Fact]
    public void ParseLine_StatusFieldIsLastWithNoTrailingDelimiter_ParsesCorrectly()
    {
        var line = "2025-03-14T08:42:11|TXN-001|USD|42.42|M-1|APPROVED";
        var (amount, status) = Memory.TransactionParser.ParseLine(line);

        Assert.Equal(42.42m, amount);
        Assert.Equal(Memory.TxnStatus.Approved, status);
    }
}

[MemoryDiagnoser]
public class TransactionParserBenchmarks
{
    private readonly string[] _lines =
    {
        "2025-03-14T08:42:11|TXN-001|USD|100.00|M-1|APPROVED",
        "2025-03-14T08:42:12|TXN-002|USD|250.50|M-1|APPROVED",
        "2025-03-14T08:42:13|TXN-003|USD|75.25|M-2|DECLINED",
        "2025-03-14T08:42:14|TXN-004|USD|10.00|M-2|APPROVED",
        "2025-03-14T08:42:15|TXN-005|USD|500.00|M-3|PENDING",
    };

    [Benchmark]
    public IReadOnlyDictionary<Memory.TxnStatus, Memory.TxnSummary> Aggregate_MultipleLines_SumsPerStatus()
        => Memory.TransactionParser.Aggregate(_lines);
}
