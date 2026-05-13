using System;
using Xunit;
using LanguageTests;

namespace LanguageTests.OOP;

public class PolymorphismBase
{
    public virtual string Test1()
    {
        return "Base";
    }
}

public class NonPolymorphicInheritance : OOP.PolymorphismBase
{
    public new string Test1()
    {
     return  "NonPolymorphicInheritance";
    }
}

public class PolymorphicInheritance : OOP.PolymorphismBase
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
        OOP.PolymorphismBase b = new OOP.NonPolymorphicInheritance();
        Assert.Equal("Base", b.Test1());

        b = new OOP.PolymorphicInheritance();
        Assert.Equal("PolymorphicInheritance", b.Test1());

    }


}