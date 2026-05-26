using System;
using System.Collections.Generic;
using Xunit;

namespace LanguageTests.Memory;

public class RefLocalsExercises
{
    // ===== Domain types used by the exercises =====

    public struct StockBin
    {
        public string Sku;
        public int Quantity;
        public decimal UnitPrice;

        public StockBin(string sku, int quantity, decimal unitPrice)
        {
            Sku = sku;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }

    public class Warehouse
    {
        // Fixed-size array of bins. Intentionally a struct array so ref semantics matter.
        public StockBin[] Bins { get; }

        public Warehouse(int capacity)
        {
            Bins = new StockBin[capacity];
        }

        // TODO (Exercise 2): implement this to return a ref to the bin matching the sku.
        public ref StockBin FindBin(string sku)
        {
            var index = Array.FindIndex(Bins, c => c.Sku == sku);
            if (index < 0) throw new KeyNotFoundException();
            return ref Bins[index];
        }
    }

    // ===== Exercises =====

    [Fact]
    public void Exercise1_AliasALocalVariable()
    {
        // SCENARIO: A stocktake correction. You receive a count for one bin and want to
        // update it through an alias rather than reassigning the original variable.
        //
        // TASK:
        //   1. Create a ref local 'alias' that aliases 'quantity'.
        //   2. Through 'alias', set the value to 42.
        //   3. Do NOT touch 'quantity' directly after declaring 'alias'.

        int quantity = 12;

        ref var alias = ref quantity;
        alias = 42;


        Assert.Equal(42, quantity);
    }

    [Fact]
    public void Exercise2_ReturnRefToStructInArray()
    {
        // SCENARIO: When a customer order arrives you want to decrement the bin's quantity.
        // Because StockBin is a struct, returning it by value would give you a COPY and
        // mutations would be lost. FindBin must return a ref.
        //
        // TASK:
        //   1. Implement Warehouse.FindBin above so it returns 'ref Bins[i]'
        //      for the bin whose Sku matches. Throw KeyNotFoundException if none match.
        //   2. Then below, use the returned ref to subtract 3 from the WIDGET bin's Quantity
        //      in a SINGLE statement (no temp variable holding a copy).

        var wh = new Warehouse(3);
        wh.Bins[0] = new StockBin("WIDGET", 100, 2.50m);
        wh.Bins[1] = new StockBin("GADGET", 50, 9.99m);
        wh.Bins[2] = new StockBin("GIZMO", 10, 19.00m);

        // >>> ADD YOUR CODE HERE <
        ref StockBin b0 = ref wh.FindBin("WIDGET");
        b0.Quantity -= 1;

        wh.FindBin("WIDGET").Quantity -= 2;

        Assert.Equal(97, wh.Bins[0].Quantity);
        Assert.Equal(50, wh.Bins[1].Quantity); // untouched
    }

    [Fact]
    public void Exercise3_RefReadonlyForSafeExposure()
    {
        // SCENARIO: A pricing service should be able to READ the unit price of a bin
        // efficiently (no struct copy) but must NOT be able to mutate it.
        //
        // TASK:
        //   1. Below, declare a local using 'ref readonly' that aliases wh.Bins[1].
        //   2. Read its UnitPrice into 'observedPrice'.
        //   3. Then UNCOMMENT the line marked TRICK and observe what the compiler says.
        //      Leave it commented for the test to pass, but write a one-line comment
        //      explaining WHY it fails.

        var wh = new Warehouse(2);
        wh.Bins[0] = new StockBin("A", 1, 1m);
        wh.Bins[1] = new StockBin("B", 2, 7.77m);

        ref readonly StockBin view = ref wh.Bins[1];


        decimal observedPrice = view.UnitPrice;

        // >>> ADD YOUR CODE HERE <
        // view.UnitPrice = 0m;  // why does this not compile?


        Assert.Equal(7.77m, observedPrice);
        Assert.Equal(7.77m, wh.Bins[1].UnitPrice); // unchanged
    }

    [Fact]
    public void Exercise4_TrickQuestion_RebindingDoesNotReseat()
    {
        // SCENARIO: A junior dev thinks they can "point" a ref local at a different
        // variable later by doing `alias = something;`. Prove them wrong (or right?).
        //
        // The test below already contains the suspect code. DO NOT modify the code in
        // the >>> REGION <<< — just predict what the asserts will be and fill in the
        // expected values. Then run the test.
        //
        // After it passes (or fails), write a comment explaining what 'alias = b;'
        // actually does on the marked line, and how you WOULD reseat a ref local
        // if the language allowed it (hint: `ref` reassignment uses different syntax).

        int a = 1;
        int b = 100;

        // >>> REGION — do not modify <
        ref int alias = ref a;
        alias = b;          // <-- what does this line do? It sets a=b. so a=100
        b = 999;
        // >>> end region <

        // Fill in the expected values:
        int expectedA =100;
        int expectedB = 999;

        Assert.Equal(expectedA, a);
        Assert.Equal(expectedB, b);
    }
}