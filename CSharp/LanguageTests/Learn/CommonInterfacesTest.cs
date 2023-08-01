using System;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class CommonInterfacesTest
    {
        public ITestOutputHelper OutputHelper { get; }
        public CommonInterfacesTest(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        [Fact]
        public void Test_IComparable()
        {
            var s = new Sale[] {
                new Sale(0.2m), new Sale(0.1m)
            };
            Array.Sort(s);
            Assert.Equal(0.1m, s[0].Percent);
            Assert.Equal(0.2m, s[1].Percent);
        }

        [Fact]
        public void Test_IComparer()
        {
            var s = new Sale[] {
                new Sale(0.2m), new Sale(0.1m)
            };

            Array.Sort(s, new SaleComparer());
            Assert.Equal(0.1m, s[0].Percent);
            Assert.Equal(0.2m, s[1].Percent);
        }
    }
}
