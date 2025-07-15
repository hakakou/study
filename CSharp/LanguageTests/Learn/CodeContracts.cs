using System;
using System.Diagnostics.Contracts;
using Xunit;
using Xunit.Abstractions;
using static LanguageTests.Demo_Records;

namespace LanguageTests
{
    public class NumbersTest(ITestOutputHelper testOutputHelper)
    {

        [Fact]
        public void Test1()
        {
            var e = long.MaxValue ;
            var  f = (int)e;
            //testOutputHelper.WriteLine($"e is {e:N0}, f is {f:N0}");
   
            e = 5_000_000_000;
            f = (int)e;
            //testOutputHelper.WriteLine($"e is {e:N0}, f is {f:N0}");

            e = 0b_01111111111111111111111111111111;
            f = (int)e;
            testOutputHelper.WriteLine($"e is {e:N0}, f is {f:N0}");
        }

    }
}
