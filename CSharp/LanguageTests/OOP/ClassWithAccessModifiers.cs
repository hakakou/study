using System;
using LanguageTests;

namespace LanguageTests.OOP;

public class ClassWithAccessModifiers
{
    private protected string PrivProt()
    {
        return "";
    }
    internal protected string IntProt()
    {
        return "";
    }
}

public class ClassWithAccessModifiersChild : OOP.ClassWithAccessModifiers
{
    public void Test()
    {
        PrivProt();
        var c = new OOP.ClassWithAccessModifiers();
        c.IntProt();
    }
}
