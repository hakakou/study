using System.Diagnostics;
using Xunit;
using LanguageTests;

namespace LanguageTests.OOP;

public interface IPlayable
{
    void Play();
    void Stop()
    {
        Trace.WriteLine("Default Implementation");
    }
}

public class DVDPlayer : OOP.IPlayable
{
    public void Play()
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        Trace.WriteLine("Override Implementation");
    }
}

public class DefaultImplentationInterface
{
    [Fact]
    public void Test()
    {
        var p = new OOP.DVDPlayer();
        p.Stop();
    }

}