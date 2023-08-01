using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;


namespace LanguageTests
{
    public class CommonTypes
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CommonTypes(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void BigNumbers()
        {
            var vlong = ulong.MaxValue;
            _testOutputHelper.WriteLine($"{vlong:N0}");  // 18.446.744.073.709.551.615

            var atoms = BigInteger.Parse("10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            _testOutputHelper.WriteLine($"{atoms:N0}");

            var ats = atoms / BigInteger.Parse("3");
            _testOutputHelper.WriteLine($"{ats:N0}");
        }

        private class ItemCollection : KeyedCollection<int, CultureInfo>
        {
            protected override int GetKeyForItem(CultureInfo item)
            {
                return item.LCID;
            }
        }

        [Fact]
        public void KeyedCollection()
        {
            var kc = new ItemCollection();
            foreach (var c in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                if (!kc.Contains(c.LCID))
                    kc.Add(c);
        }

        [Fact]
        public void SortedSet()
        {
            var kc = new SortedSet<string>();
            kc.Add("Z");
            kc.Add("A");
        }

        [Fact]
        public void LinkedListTest()
        {
            string[] words =
                { "the", "fox", "jumps", "over", "the", "dog" };
            LinkedList<string> sentence = new LinkedList<string>(words);
            sentence.AddFirst("today");
            var current = sentence.Find("jumps");
            sentence.AddAfter(current, "test");
        }

        [Fact]
        public void SpanTest()
        {
            var list = Enumerable.Range(0, 10).ToArray(); // 0..9
            ReadOnlySpan<int> slist = list.AsSpan();

            var range = slist[0..3];    // a
            Assert.Equal(new int[] { 0, 1, 2 }, range.ToArray());

            range = slist[^1..^0];  // b
            Assert.Equal(new int[] { 9 }, range.ToArray());

            range = slist[1..1];    // c
            Assert.Equal(0, range.Length);

            var v = slist[^1];  //d
            Assert.Equal(9, v);

            // v = slist[^0]; //e
            // System.IndexOutOfRangeException : Index was outside the bounds of the array.

            Index ind = new Index(10, true); // f
            Assert.Equal(0, slist[ind]);
        }
    }
}
