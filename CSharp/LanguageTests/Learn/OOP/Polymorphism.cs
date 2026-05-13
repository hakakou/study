using System;
using Xunit;

namespace LanguageTests
{
    public class PolymorphismBase
    {
        public virtual string Test1()
        {
            return "Base";
        }
    }

    public class NonPolymorphicInheritance : PolymorphismBase
    {
        public new string Test1()
        {
         return  "NonPolymorphicInheritance";
        }
    }

    public class PolymorphicInheritance : PolymorphismBase
    {
        public override string Test1()
        {
           return "PolymorphicInheritance";
        }
    }

    public class Polymorphism
    {
        public static string Temp;

        [Fact]
        void Test()
        {
            PolymorphismBase b = new NonPolymorphicInheritance();
            Assert.Equal("Base", b.Test1());

            b = new PolymorphicInheritance();
            Assert.Equal("PolymorphicInheritance", b.Test1());

        }


    }

}