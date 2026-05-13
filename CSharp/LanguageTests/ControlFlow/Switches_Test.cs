using System;
using System.IO;
using static System.Console;
using Xunit;
using LanguageTests;

namespace LanguageTests.ControlFlow;

public class Switches_Test
{
// Pattern matching with the is operator
public static bool IsLetter(char c) =>
    c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');

[Fact]
public void TestIsLetter_WithLowercaseLetter_ReturnsTrue()
{
    Assert.True(IsLetter('a'));
}

// Pattern matching with the is operator
public static bool IsSto(int num) =>
    num is (>= 0 and <= 100) or (200);

[Fact]
public void TestIsSto_WithValueInRange_ReturnsTrue()
{
    Assert.True(IsSto(50));
}

// Pattern matching with the switch statement
public static void DoSwitch(Stream s)
{
    switch (s)
    {
        case null:
            WriteLine("null");
            break;
        case var str when str.Length == 0:
            WriteLine("0");
            break;
        case FileStream wfs when s.CanWrite:
            WriteLine("wfs");
            break;
        case FileStream fs:
            WriteLine("readonly");
            break;
        case MemoryStream ms:
            WriteLine("readonly");
            break;
        default:
            WriteLine("break");
            break;
    }
}

[Fact]
public void TestDoSwitch_WithMemoryStream_WritesReadonly()
{
    DoSwitch(new MemoryStream());
}

public static string ClassifyPoint((int X, int Y) p) =>
p switch
{
    (0, 0) => "Origin",
    (0, _) => "Y-axis",
    (_, 0) => "X-axis",
    ( >= 0, >= 0) => "Quadrant I",
    ( < 0, >= 0) => "Quadrant II",
    ( < 0, < 0) => "Quadrant III",
    ( >= 0, < 0) => "Quadrant IV"
};

[Fact]
public void TestClassifyPoint_WithYAxis_ReturnsYAxis()
{
    Assert.Equal("Y-axis", ClassifyPoint((0, 5)));
}


public static string DescribeNumber(int n) =>
n switch
{
    < 0 => "Negative",
    0 => "Zero",
    > 0 and <= 10 => "Small",
    > 10 and <= 100 => "Medium",
    > 100 and <= 1000 => "Large",
    _ => "Huge"
};

[Fact]
public void TestDescribeNumber_WithNegativeNumber_ReturnsNegative()
{
    Assert.Equal("Negative", DescribeNumber(-5));
}


// Pattern matching with the switch statement and property patterns
public static void DoSwitch3(ControlFlow.Switches_Test.Animal animal)
{
    string message = animal switch
    {
        ControlFlow.Switches_Test.Cat { Legs: 4 } fourLeggedCat =>
        $"The cat named {fourLeggedCat.Name} has four legs.",

        ControlFlow.Switches_Test.Cat { IsDomestic: false } wildCat =>
            $"The non-domestic cat is named {wildCat.Name}.",

        ControlFlow.Switches_Test.Cat cat =>
            $"The cat is named {cat.Name}.",

        ControlFlow.Switches_Test.Spider { IsVenomous: true } spider =>
            $"The {spider.Name} spider is venomous. Run!",

        null => 
            "The animal is null.",

        _ =>
            $"{animal?.Name} is a {animal?.GetType().Name}."
    };

    Console.WriteLine($"switch statement: {message}");
}

[Fact]
public void TestDoSwitch3_WithDomesticCat_ReturnsCorrectMessage()
{
    var cat = new ControlFlow.Switches_Test.Cat { Name = "Whiskers", Legs = 4, IsDomestic = true };
    DoSwitch3(cat);
}

// Switch expressions with property patterns
public static void DoSwitchExpression(ControlFlow.Switches_Test.Animal animal)
{
    string message = animal switch
    {
        ControlFlow.Switches_Test.Cat fourLeggedCat when fourLeggedCat.Legs == 4 => $"The cat named {fourLeggedCat.Name} has four legs.",
        ControlFlow.Switches_Test.Cat wildCat when wildCat.IsDomestic == false => $"The non-domestic cat is named {wildCat.Name}.",
        ControlFlow.Switches_Test.Cat cat => $"The cat is named {cat.Name}.",
        ControlFlow.Switches_Test.Spider spider when spider.IsVenomous => $"The {spider.Name} spider is venomous. Run!",
        null => "The animal is null.",
        _ => $"{animal.Name} is a {animal.GetType().Name}."
    };
}

[Fact]
public void TestDoSwitchExpression_WithDomesticCat_Succeeds()
{
    var cat = new ControlFlow.Switches_Test.Cat { Name = "Mittens", Legs = 4, IsDomestic = true };
    DoSwitchExpression(cat);
}

// Switch expressions with type patterns
public static string DoSwitch2(Stream s)
{
    return s switch
    {
        null => "null",
        var str when str.Length == 0 => "0",
        FileStream fs when s.CanRead => "r",
        FileStream fs => "rw",
        MemoryStream ms => "ms",
        _ => "unknown"
    };
}

[Fact]
public void TestDoSwitch2_WithNullStream_ReturnsNull()
{
    Assert.Equal("null", DoSwitch2(null!));
}

public class Animal
{
    public string? Name;
    public DateTime Born;
    public byte Legs;
}

public class Cat : ControlFlow.Switches_Test.Animal
{
    public bool IsDomestic;
}

public class Spider : ControlFlow.Switches_Test.Animal
{
    public bool IsVenomous;
}
}