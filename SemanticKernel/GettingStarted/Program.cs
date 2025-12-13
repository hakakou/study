using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;
using System.Reflection;

Console.Clear();
Console.OutputEncoding = System.Text.Encoding.UTF8;
Conf.Init<Program>();

var builder = Host.CreateApplicationBuilder(args);

// OpenTelemetry setup
var endpoint = "http://localhost:4317";

var resourceBuilder = ResourceBuilder
    .CreateDefault().AddService("Study");

// Enable model diagnostics with sensitive data.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

using var traceProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource("Microsoft.SemanticKernel*")
    .AddOtlpExporter(options => options.Endpoint = new Uri(endpoint))
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddMeter("Microsoft.SemanticKernel*")
    .AddOtlpExporter(options => options.Endpoint = new Uri(endpoint))
    .Build();

builder.Services.AddLogging(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
        // Format log messages. This is default to false.
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Information);
});

/// End of OpenTelemetry setup

var testTypes = Assembly.GetExecutingAssembly().GetTypes()
    .Where(t => (typeof(ITest).IsAssignableFrom(t))
        && !t.IsInterface && !t.IsAbstract)
    .OrderByDescending(q => q.Name)
    .ToList();

foreach (var type in testTypes)
{
    builder.Services.AddTransient(type);
}

var directRunType = testTypes.SingleOrDefault(t => t.GetCustomAttribute<RunDirectlyAttribute>() != null);
if (directRunType != null)
{
    await RunTestType(directRunType);
}
else
{
    var selectedFunction = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select a function to run")
            .AddChoices(testTypes.Select(t => t.Name).ToArray()));

    var selectedType = testTypes.FirstOrDefault(t => t.Name == selectedFunction);
    if (selectedType != null)
    {
        await RunTestType(selectedType);
    }
}

Console.WriteLine();

async Task RunTestType(Type testType)
{
    testType.GetMethod("Build")?.Invoke(null, new object[] { builder.Services });
    
    var host = builder.Build();
    try
    {
        var test = (host.Services.GetRequiredService(testType) as ITest)!;
        await test.Run();
    }
    finally
    {
        await host.StopAsync();
    }
}
