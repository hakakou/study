using System.Diagnostics;
using Xunit;

namespace LanguageTests;

public interface IPlayable
{
    void Play();
    void Stop()
    {
        Trace.WriteLine("Default Implementation");
    }
}

public class DVDPlayer : IPlayable
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
        var p = new DVDPlayer();
        p.Stop();
    }

}