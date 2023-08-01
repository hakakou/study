using System;
using System.Collections;
using System.Security.Claims;
using System.Threading;
using Haka.Lib;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests;

public class LoopsTest(ITestOutputHelper t)
{

    [Fact]
    public void TestSign()
    {
        var abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        IEnumerator enumerator = abc.GetEnumerator();
        while (enumerator.MoveNext())
        {
            char c = (char)enumerator.Current;
            t.WriteLine(c.ToString());
        }
    }
}
