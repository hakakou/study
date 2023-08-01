using System;
using System.Collections;
using System.Security.Claims;
using System.Threading;
using AwesomeAssertions;
using Haka.Lib;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests;

public class ArrayTests(ITestOutputHelper t)
{

    [Fact]
    public void TestMultiDimensional()
    {
        string[,] grid =
        {
            { "0,0", "0,1", "0,2", "0,3" },
            { "1,0", "1,1", "1,2", "1,3" },
        };

        for (int row = grid.GetLowerBound(0); row <= grid.GetUpperBound(0); row++)
        // 0..1
        {
            for (int col = grid.GetLowerBound(1); col <= grid.GetUpperBound(1); col++)
            // 0..3
            {
                t.WriteLine($"[r{row},c{col}] = {grid[row, col]}");
            }
        }

        grid.GetLowerBound(0).Should().Be(0);
        grid.GetUpperBound(0).Should().Be(1);
        grid.GetLowerBound(1).Should().Be(0);
        grid.GetUpperBound(1).Should().Be(3);
    }


    [Fact]
    public void TestJaggedArray()
    {
        string[][] jagged =
        {
            ["0,0", "0,1", "0,2"],
            ["1,1", "1,2", "1,3", "1,4"],
            [ ],
            ["3,0"]
        };

        for (int row = 0; row <= jagged.GetUpperBound(0); row++)
        // 0..3
        {
            for (int col = 0; col <= jagged[row].GetUpperBound(0); col++)
            // 0..2, 0..3, 0..-1, 0..0
            {
                t.WriteLine($"[r{row},c{col}] = {jagged[row][col]}");
            }

        }
    }
}
