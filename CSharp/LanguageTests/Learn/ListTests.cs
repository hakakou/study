using AwesomeAssertions;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests;

public class ListTests(ITestOutputHelper t)
{

    [Fact]
    public void ListPatternMatching_Test()
    {
        int[] sequentialNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int[] oneTwoNumbers = { 1, 2 };
        int[] oneTwoTenNumbers = { 1, 2, 10 };
        int[] oneTwoThreeTenNumbers = { 1, 2, 3, 10 };
        int[] primeNumbers = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
        int[] fibonacciNumbers = { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89 };
        int[] emptyNumbers = { }; // Or use Array.Empty<int>()
        int[] threeNumbers = { 9, 7, 5 };
        int[] sixNumbers = { 9, 7, 5, 4, 2, 10 };

        (new int[] { } is []).Should().Be(true);

        (new int[] { } is [..]).Should().Be(true);
        (new int[] { 1, 2 } is [..]).Should().Be(true);

        (new int[] { 1, 2, 3, 10 } is [1, 2, _, 10]).Should().Be(true);

        (new int[] { 1, 2 } is [_, _]).Should().Be(true);

        (new int[] { 1, 2, 3 } is [_, int i2, _] && i2 == 2).Should().Be(true);

        (new int[] { 1, 2, 3 } is [.., int i3] && i3 == 3).Should().Be(true);

        (new int[] { 1, 2, 3 } is [1, .. int[] ot, 3] && ot.Length == 1).Should().Be(true);

        static string Check(int[] values) => values switch
        {
            [] => "[]",
            [1, 2, _, 10] => "[1, 2, _, 10]",
            _ => ""
        };
    }
}
