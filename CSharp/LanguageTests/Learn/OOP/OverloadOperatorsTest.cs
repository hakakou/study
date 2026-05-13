using Xunit;

namespace LanguageTests
{
    public class OverloadOperatorsTest
    {
        [Fact]
        public void Test()
        {
            var s1 = new Sale(0);
            s1 += 0.1m;

            var s2 = new Sale(0.1m);

            Assert.True(s1 == s2);
            Assert.Equal(0.1m, s1.Percent);

            var r = new Sale(0.2m - 0.1m * 0.1m);
            Assert.True((s1 + s2).Percent == r.Percent);
            Assert.True((s1 + s2) == r);
        }
    }
}


