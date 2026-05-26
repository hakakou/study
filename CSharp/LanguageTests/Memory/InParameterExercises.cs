using System;
using System.Diagnostics;
using Xunit;

namespace LanguageTests.Memory;

public class InParameterExercises
{
    private readonly ITestOutputHelper _output;
    public InParameterExercises(ITestOutputHelper output) => _output = output;

    // ===== Two structs that LOOK almost identical but behave very differently =====

    // A "fat" struct — large enough that copying is measurably expensive.
    // NOT marked readonly. Has a method that doesn't mutate state but the compiler
    // doesn't know that.
    public struct Trade
    {
        public long Id;
        public decimal Price;
        public decimal Quantity;
        public long TimestampTicks;
        public Guid AccountId;
        public Guid InstrumentId;
        public int Venue;
        public int Side;
        public decimal Commission;
        public decimal Tax;

        // How many times this method has been called. Used to detect copies.
        public static int NotionalCallCount;

        // Calculates notional value. Does NOT mutate the struct — but it's not
        // marked 'readonly', so the compiler must assume it might.
        public decimal Notional()
        {
            NotionalCallCount++;
            return Price * Quantity;
        }
    }

    // Same fields, but the struct AND its method are marked readonly.
    public readonly struct ReadonlyTrade
    {
        public readonly long Id;
        public readonly decimal Price;
        public readonly decimal Quantity;
        public readonly long TimestampTicks;
        public readonly Guid AccountId;
        public readonly Guid InstrumentId;
        public readonly int Venue;
        public readonly int Side;
        public readonly decimal Commission;
        public readonly decimal Tax;

        public static int NotionalCallCount;

        public ReadonlyTrade(decimal price, decimal quantity)
        {
            Id = 0;
            Price = price;
            Quantity = quantity;
            TimestampTicks = 0;
            AccountId = default;
            InstrumentId = default;
            Venue = 0;
            Side = 0;
            Commission = 0;
            Tax = 0;
        }

        public decimal Notional()
        {
            NotionalCallCount++;
            return Price * Quantity;
        }
    }

    // ===== Exercises =====

    [Fact]
    public void Exercise1_InParameterBasicUsage()
    {
        // SCENARIO: Pass a Trade by readonly reference to avoid copying ~80 bytes
        // on every call.
        //
        // TASK:
        //   1. Implement the local function 'CalcFee' below so it takes the trade
        //      as an 'in' parameter and returns price * quantity * 0.001m.
        //   2. Call it on 'trade' WITHOUT writing the 'in' keyword at the call site
        //      (it's optional for in-params — verify this works).
        //   3. Then call it AGAIN, this time WITH 'in' at the call site.
        //   4. Both calls should give the same result.

        var trade = new Trade
        {
            Price = 100m,
            Quantity = 50m
        };

        // >>> IMPLEMENT THIS LOCAL FUNCTION <
        decimal CalcFee(in Trade t) => t.Price * t.Quantity * 0.001m;

        decimal fee1 = CalcFee(trade);
        decimal fee2 = CalcFee(in  trade);

        Assert.Equal(5m, fee1);
        Assert.Equal(5m, fee2);
    }

    [Fact]
    public void Exercise2_TheDefensiveCopyTrap()
    {
        // SCENARIO: You "optimised" a hot path by changing `Trade` to be passed `in`.
        // Profiling shows... no improvement. Worse, allocations of stack copies are
        // happening anyway. Why?
        //
        // The method 'SumNotionalsBuggy' below uses an in-parameter and calls
        // trade.Notional() inside a loop. Because Trade is NOT a readonly struct
        // and Notional() is NOT a readonly method, the compiler is forced to make
        // a DEFENSIVE COPY of the struct on every call — to protect the caller's
        // value from being mutated by Notional().
        //
        // TASK:
        //   1. Run the test as-is and observe: NotionalCallCount == iterations,
        //      but the elapsed time and the assertion below will reveal the copy.
        //   2. Look at SumNotionalsFixed — it's identical in shape but uses
        //      ReadonlyTrade. No defensive copy is made.
        //   3. Predict, then assert, which version runs faster.
        //   4. Trick part: change ONE thing about the Trade struct (without
        //      converting it to ReadonlyTrade) that would also eliminate the
        //      defensive copy. Write your answer as a comment.

        const int iterations = 10_000_000;

        var mutableTrade = new Trade { Price = 100m, Quantity = 50m };
        var readonlyTrade = new ReadonlyTrade(100m, 50m);

        Trade.NotionalCallCount = 0;
        ReadonlyTrade.NotionalCallCount = 0;

        var sw1 = Stopwatch.StartNew();
        decimal sumBuggy = SumNotionalsBuggy(in mutableTrade, iterations);
        sw1.Stop();

        var sw2 = Stopwatch.StartNew();
        decimal sumFixed = SumNotionalsFixed(in readonlyTrade, iterations);
        sw2.Stop();

        _output.WriteLine($"Buggy (mutable struct, in-param):    {sw1.ElapsedMilliseconds} ms {Trade.NotionalCallCount}");
        _output.WriteLine($"Fixed (readonly struct, in-param):   {sw2.ElapsedMilliseconds} ms {ReadonlyTrade.NotionalCallCount}");

        Assert.Equal(sumBuggy, sumFixed); // same result
        Assert.True(sw2.ElapsedMilliseconds <= sw1.ElapsedMilliseconds,
            "Readonly struct should be at least as fast (usually significantly faster).");

        // >>> WRITE YOUR ANSWER AS A COMMENT <
        // What ONE change to `Trade` (keeping it a non-readonly struct) would
        // also eliminate the defensive copy when calling Notional()?
        // ANSWER: public readonly decimal Notional()
    }

    private static decimal SumNotionalsBuggy(in Trade trade, int iterations)
    {
        decimal sum = 0m;
        for (int i = 0; i < iterations; i++)
        {
            // Each call here forces a defensive copy of `trade` because:
            //   - `trade` is an in-parameter (readonly reference)
            //   - Notional() is not marked readonly
            //   - Trade itself is not a readonly struct
            sum += trade.Notional();
        }
        return sum;
    }

    private static decimal SumNotionalsFixed(in ReadonlyTrade trade, int iterations)
    {
        decimal sum = 0m;
        for (int i = 0; i < iterations; i++)
        {
            sum += trade.Notional();
        }
        return sum;
    }

    [Fact]
    public void Exercise3_TrickQuestion_MutationVisibility()
    {
        // SCENARIO: A teammate insists that because `in` parameters are passed by
        // reference, mutations to the underlying variable should be visible
        // inside the method. They write the code below to "prove" it.
        //
        // Predict the result BEFORE running. Fill in `expected` with what you
        // think the value will be.
        //
        // Hint: there are TWO effects fighting each other here:
        //   (a) `in` is pass-by-reference, so mutations to the caller's variable
        //       *should* be observable inside the method.
        //   (b) But Trade is a non-readonly struct, and `Snapshot` reads a field
        //       via a method (well, a property would be the same) — there's a
        //       defensive copy lurking.
        //
        // Actually, the snippet below reads a field DIRECTLY, not via a method.
        // Does that change anything? Predict, then run.

        var trade = new Trade { Price = 100m };
        decimal snapshot = Snapshot(trade, () =>
        {
            trade.Price = 999m;
        });

        // Fill in:
        decimal expected = 999m;
        Assert.Equal(expected, snapshot);

        // Now write a comment: was your prediction right? Why does direct field
        // access behave differently from a method call on an in-parameter?
        // EXPLANATION:
        // Both the below mutate the price to 999:
        // Snapshot(in trade, () => trade.Price = 999m);
        // Snapshot(trade, () => trade.Price = 999m);  
        // What is the difference?
        // However if I change the declaration to 
        // private static decimal Snapshot(Trade t, Action mutateCaller)
        // then the mutation does not work
    }

    private static decimal Snapshot(in Trade t, Action mutateCaller)
    {
        // Read the field BEFORE the mutation.
        decimal before = t.Price;

        // t.Price = 0;  compile error when "in" and not "ref"

        // Mutate the caller's variable via the closure.
        mutateCaller();

        // Read the field AFTER the mutation. Is `before` == `after`?
        decimal after = t.Price;

        // Return `after` so the test can assert on what the method observed
        // post-mutation.
        return after;
    }
}