using System;
using System.Diagnostics;
using Xunit;

namespace LanguageTests
{
    public class TuplesTest
    {
        public (string, int) GetFruit()
        {
            return ("Apple", 5);
        }

        public (string Fruit, int Number) GetNamedFruit()
        {
            return (Fruit: "Apple", Number: 5);
            // return ("Apple", 5);
        }

        [Fact]
        public void Test()
        {
            (string, int) fruit = GetFruit();
            var fruit2 = GetFruit();

            Assert.Equal(5, fruit.Item2);
            Assert.Equal("ValueTuple`2", fruit.GetType().Name);
        }

        [Fact]
        public void TestNamed()
        {
            var fruit = GetNamedFruit();

            Assert.Equal("Apple", fruit.Fruit);
            Assert.Equal("Apple", fruit.Item1);
            Assert.Equal("ValueTuple`2", fruit.GetType().Name);

            var thing2 = (Environment.MachineName, Environment.Version.Major);
            var inferringTupleName = $"{thing2.MachineName} {thing2.Major}";
            var toString = thing2.ToString(); // (BIG, 3)

            (string str1, int num1) = ("aa", 2);
            (string str2, int num2) = GetFruit();
            (string str3, int num3) = GetNamedFruit();

            Trace.WriteLine($"{str1} {num1}");
        }

    }


}