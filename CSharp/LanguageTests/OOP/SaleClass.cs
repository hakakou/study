using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LanguageTests;

namespace LanguageTests.OOP;

public class Sale : IComparable<OOP.Sale>
{
    public decimal Percent { get; set; }
    
    public Sale(decimal percent)
    {
        Percent = percent;
    }

    public override bool Equals(object obj)
    {
        return obj is OOP.Sale sale &&
               Percent == sale.Percent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Percent);
    }

    // Overloaded operators

    public static bool operator ==(OOP.Sale p1, OOP.Sale p2)
    {
        return p1.Percent == p2.Percent;
    }
    public static bool operator !=(OOP.Sale p1, OOP.Sale p2)
    {
        return p1.Percent != p2.Percent;
    }

    public static OOP.Sale operator +(OOP.Sale p1, decimal amount)
    {
        return new OOP.Sale(p1.Percent + amount);
    }
    public static OOP.Sale operator +(OOP.Sale p1, OOP.Sale p2)
    {
        return new OOP.Sale(p1.Percent + p2.Percent - p1.Percent * p2.Percent);
    }

    // IComparable<Sale>

    public int CompareTo([AllowNull] OOP.Sale other)
    {
        return decimal.Compare(Percent, other.Percent);
    }
}

public class SaleComparer : IComparer<OOP.Sale>
{
    public int Compare([AllowNull] OOP.Sale x, [AllowNull] OOP.Sale y)
    {
        return x.CompareTo(y);
    }
}