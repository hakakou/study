using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LanguageTests;

public class TraceConfigurationTests
{
    [Fact]
    public void TraceConfiguration_should_load_from_appconfig()
    {
        using var writer = File.CreateText(@"c:\unzip\log.txt");
        using var listener = new TextWriterTraceListener(writer);
        Trace.Listeners.Add(listener);

        Trace.AutoFlush = true;

        var builder = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile(@"appconfig.json",
                optional: false, reloadOnChange: true);

        var configuration = builder.Build();

        var ts = new TraceSwitch(displayName: "PacktSwitch", description: "From Config");
        configuration.GetSection("PacktSwitch").Bind(ts);

        Trace.WriteLineIf(ts.TraceError, "Error");
        Trace.WriteLineIf(ts.TraceWarning, "Warning,");
        Trace.WriteLineIf(ts.TraceInfo, "Info");
        Trace.WriteLineIf(ts.TraceVerbose, "Verbose");

        BooleanSwitch dataSwitch = new BooleanSwitch("boolSwitch", "DataAccess module");
        //configuration.GetSection("BoolSwitch").Bind(dataSwitch);

        Assert.False(dataSwitch.Enabled);

        Trace.Listeners.Remove(listener);
    }
}
