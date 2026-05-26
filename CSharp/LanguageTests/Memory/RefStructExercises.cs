using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LanguageTests.Memory;

public class RefStructExercises
{
    // ===== Domain types =====

    // A parsed log line that BORROWS slices of the original buffer.
    // No string allocations — each field is a ReadOnlySpan<char> window.
    public ref struct LogEntry
    {
        public ReadOnlySpan<char> Timestamp;
        public ReadOnlySpan<char> Level;
        public ReadOnlySpan<char> Message;
    }

    // Sample log format: "2025-11-04T10:15:23Z|INFO|User logged in"
    private const string SampleLog =
        "2025-11-04T10:15:23Z|INFO|User logged in\n" +
        "2025-11-04T10:15:24Z|WARn|Slow query detected\n" +
        "2025-11-04T10:15:25Z|ERROR|Database connection lost";


    // TODO (Exercise 2): implement this parser.
    // It should split 'line' on '|' into three spans and return a LogEntry
    // whose fields are SLICES of 'line' (no ToString, no new strings).
    public static LogEntry ParseLine(ReadOnlySpan<char> line)
    {
        LogEntry ls = default;

        int i = line.IndexOf('|');
        ls.Timestamp = line[0..i];
        line = line[(i + 1)..];

        i = line.IndexOf('|');
        ls.Level = line[0..i];
        line = line[(i + 1)..];

        ls.Message = line;

        return ls;
    }

    // ===== Exercises =====

    [Fact]
    public void Exercise1_SpanSlicingWithoutAllocation()
    {
        string line = "2025-11-04T10:15:23Z|INFO|User logged in";

        ReadOnlySpan<char> span = line.AsSpan();
        var i = span.IndexOf('|');
        // IndexOf on a ReadOnlySpan<char> is heavily vectorized (uses SIMD on platforms that support it). 

        span = span[(i + 1)..];
        // Reusing span as a moving window is a pattern you'd see in production parsers. 

        i = span.IndexOf('|');

        ReadOnlySpan<char> levelSpan = span[..i];

        Assert.True(levelSpan.SequenceEqual("INFO".AsSpan()));
        Assert.Equal(4, levelSpan.Length);
    }

    [Fact]
    public void Exercise2_ParseIntoRefStruct()
    {
        ReadOnlySpan<char> buffer = SampleLog.AsSpan();

        var firstLine = buffer.IndexOf('\n');
        LogEntry entry = ParseLine(buffer[0..firstLine]);

        Assert.True(entry.Timestamp.SequenceEqual("2025-11-04T10:15:23Z".AsSpan()));
        Assert.True(entry.Level.SequenceEqual("INFO".AsSpan()));
        Assert.True(entry.Message.SequenceEqual("User logged in".AsSpan()));
    }

    [Fact]
    public void Exercise3_StackallocForScratchBuffer()
    {
        // SCENARIO: You need to UPPERCASE the level field for normalised comparison,
        // but only for the duration of this method. Allocating a string would defeat
        // the purpose of using spans. Use stackalloc to get scratch memory.
        //
        // TASK:
        //   1. Parse the second line of SampleLog (level = "WARN").
        //   2. Allocate a Span<char> on the stack big enough for the level
        //      using `stackalloc char[N]`.
        //   3. Copy entry.Level into it, then uppercase it in-place
        //      (it's already uppercase here, so this is mostly mechanical —
        //      use char.ToUpperInvariant in a for-loop over the span).
        //   4. Assert the scratch span equals "WARN".

        ReadOnlySpan<char> buffer = SampleLog.AsSpan();

        // Find line 2: skip past the first '\n'.
        int firstNewline = buffer.IndexOf('\n');
        ReadOnlySpan<char> line2 = buffer.Slice(firstNewline + 1);
        int secondNewline = line2.IndexOf('\n');
        line2 = line2.Slice(0, secondNewline);

        LogEntry entry = ParseLine(line2);

        Span<char> scratch = stackalloc char[entry.Level.Length];
        for (int i = 0; i < entry.Level.Length; i++)
        {
            scratch[i] = char.ToUpperInvariant(entry.Level[i]); 
        }
        
        Assert.True(scratch.SequenceEqual("WARN".AsSpan()));
    }

    [Fact]
    public async Task Exercise4_TrickQuestion_RefStructInAsync()
    {
        // SCENARIO: A teammate wants to "make the parser async" so it can be awaited
        // alongside an IO call. They write the code below. Predict what happens
        // BEFORE you try to compile it.
        //
        // TASK:
        //   1. UNCOMMENT the marked block.
        //   2. Try to build. Write a comment explaining the exact compiler error
        //      and WHY the language forbids this.
        //   3. Then leave it commented so the test compiles, and instead just
        //      assert true below (this exercise is about the error, not the runtime).
        //
        // Hint: think about where local variables live when a method is async.

        await Task.Yield();
   
        ReadOnlySpan<char> line = "2025-11-04T10:15:23Z|INFO|hello".AsSpan();
        LogEntry entry = ParseLine(line);
        // await Task.Delay(1);
        var level = entry.Level;

        // Your one-line explanation as a comment:
        // if I comment in the await Task.Delay(1); line above, I get an error instance of type xxx cannot be preserved across await or yield boundary 
        Assert.True(true);
    }
}