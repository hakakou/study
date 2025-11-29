using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;
using System.Reflection;

Console.Clear();

Conf.Init<Program>();

var services = new ServiceCollection();

// OpenTelemetry setup
var endpoint = "http://localhost:4317";

var resourceBuilder = ResourceBuilder
    .CreateDefault().AddService("Study");
// autoGenerateServiceInstanceId: false

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

services.AddLogging(builder =>
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
    if (typeof(ITestBuilder).IsAssignableFrom(type))
    {
        type.GetMethod("Build")?.Invoke(null, [services]);
    }
    services.AddTransient(type);
}

var serviceProvider = services.BuildServiceProvider();

var directRunType = testTypes.SingleOrDefault(t => t.GetCustomAttribute<RunDirectlyAttribute>() != null);
if (directRunType != null)
{
    var test = (serviceProvider.GetRequiredService(directRunType) as ITest)!;
    await test.Run();
    if (test is ITestBuilder t)
        await t.Run(serviceProvider);
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
        var test = (serviceProvider.GetRequiredService(selectedType) as ITest)!;
        await test.Run();
        if (test is ITestBuilder t)
            await t.Run(serviceProvider);
    }
}

Console.WriteLine();
