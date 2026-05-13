using System;
using System.Linq;
using Xunit;

namespace LanguageTests;

public class FlagsTest
{
    [Flags]
    public enum Bucket : byte
    {
        item1 = 1 << 0,
        item2 = 1 << 1,
        item3 = 1 << 2,
    }

    [Fact]
    public void Test1()
    {
        var items = Bucket.item1 | Bucket.item3;
        var str = items.ToString();
        Assert.Equal("item1, item3", str);

        Assert.False(items.HasFlag(Bucket.item2));

        items |= Bucket.item2;
        Assert.True(items.HasFlag(Bucket.item2));

        items &= ~Bucket.item1;
        Assert.False(items.HasFlag(Bucket.item1));

        Assert.Equal("item2, item3", items.ToString());
    }
}