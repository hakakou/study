using System.Collections.Generic;
using Xunit;
using LanguageTests;

namespace LanguageTests.OOP;

public class ClassWithIndexer
{
    private Dictionary<int, string> children =
     new Dictionary<int, string>();

    public string this[int index]
    {
        get
        {
            if (children.TryGetValue(index, out string str))
                return str;
            else
                return null;
        }
        set
        {
            children[index] = value;
        }
    }
}

public class IndexerTest
{
    public ITestOutputHelper OutputHelper { get; }

    public IndexerTest(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    [Fact]
    public void Test()
    {
        var cl = new OOP.ClassWithIndexer();
        cl[10] = "HHH";
        cl[2] = "HHH";
        Assert.Equal("HHH", cl[2]);
        Assert.Null(cl[11]);
    }
    
}
