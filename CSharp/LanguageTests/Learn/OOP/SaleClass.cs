using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LanguageTests
{
    public class Sale : IComparable<Sale>
    {
        public decimal Percent { get; set; }
        
        public Sale(decimal percent)
        {
            Percent = percent;
        }

        public override bool Equals(object obj)
        {
            return obj is Sale sale &&
                   Percent == sale.Percent;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Percent);
        }

        // Overloaded operators

        public static bool operator ==(Sale p1, Sale p2)
        {
            return p1.Percent == p2.Percent;
        }
        public static bool operator !=(Sale p1, Sale p2)
        {
            return p1.Percent != p2.Percent;
        }

        public static Sale operator +(Sale p1, decimal amount)
        {
            return new Sale(p1.Percent + amount);
        }
        public static Sale operator +(Sale p1, Sale p2)
        {
            return new Sale(p1.Percent + p2.Percent - p1.Percent * p2.Percent);
        }

        // IComparable<Sale>

        public int CompareTo([AllowNull] Sale other)
        {
            return decimal.Compare(Percent, other.Percent);
        }
    }

    public class SaleComparer : IComparer<Sale>
    {
        public int Compare([AllowNull] Sale x, [AllowNull] Sale y)
        {
            return x.CompareTo(y);
        }
    }

}