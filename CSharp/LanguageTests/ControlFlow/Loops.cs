using System;
using System.Collections;
using Xunit;

namespace LanguageTests.ControlFlow;

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
