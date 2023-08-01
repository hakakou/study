using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace LanguageTests
{
    // XUnit calls this!
    public class Startup
    {
        public Startup()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(
                File.CreateText(@"c:\unzip\log.txt")));

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

            Trace.Assert(dataSwitch.Enabled == false);
        }
    }
}
