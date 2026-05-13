using System;
using System.Linq;
using Xunit;
using LanguageTests;

namespace LanguageTests.Types;

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
        var items = Types.FlagsTest.Bucket.item1 | Types.FlagsTest.Bucket.item3;
        var str = items.ToString();
        Assert.Equal("item1, item3", str);

        Assert.False(items.HasFlag(Types.FlagsTest.Bucket.item2));

        items |= Types.FlagsTest.Bucket.item2;
        Assert.True(items.HasFlag(Types.FlagsTest.Bucket.item2));

        items &= ~Types.FlagsTest.Bucket.item1;
        Assert.False(items.HasFlag(Types.FlagsTest.Bucket.item1));

        Assert.Equal("item2, item3", items.ToString());
    }
}