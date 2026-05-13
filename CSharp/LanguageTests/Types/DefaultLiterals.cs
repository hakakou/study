using System;
using System.Collections.Generic;
using Xunit;
using LanguageTests;

namespace LanguageTests.Types;

public class DefaultLiterals
{
    [Fact]
    public void Test1()
    {
        // Default Literals
        int Int = default;
        DateTime dt = default;
        List<string> list = default;

        Assert.Equal(0, Int);
        Assert.Equal(DateTime.MinValue, dt);
        Assert.Equal(null, list);
    }
}