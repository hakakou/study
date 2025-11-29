using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

#pragma warning disable SKEXP0001

public class DOC_S12_ApplicationInsights : ITest
{
    public async Task Run()
    {
        var endpoint = "http://localhost:4317";
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService("TelemetryAspireDashboardQuickstart");

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

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            // Add OpenTelemetry as a logging provider
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
    }

    /// </summary>
    private async Task BasicApplicationInsightsExample()
    {
        Utils.PrintSectionHeader("=== Example 1: Basic Application Insights Integration ===\n");

        // NOTE: Replace with your actual Application Insights connection string
        // You can find this in Azure Portal > Application Insights > Overview > Connection String
        // Create a resource builder that identifies your service in Application Insights
        // This helps you filter and organize telemetry data by service name
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService("GettingStarted");

        // This will log prompts and completions to Application Insights.
        AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

        // Configure OpenTelemetry tracing (distributed traces and dependencies)
        using var traceProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource("Microsoft.SemanticKernel*") // Capture all Semantic Kernel traces
            .AddAzureMonitorTraceExporter(options => options.ConnectionString = Conf.ApplicationInsights.ConnectionString)
            .Build();

        // Configure OpenTelemetry metrics (performance counters, custom metrics)
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("Microsoft.SemanticKernel*") // Capture all Semantic Kernel metrics
            .AddAzureMonitorMetricExporter(options => options.ConnectionString = Conf.ApplicationInsights.ConnectionString)
            .Build();

        // Configure logging with OpenTelemetry integration
        // Logs provide detailed diagnostic information about application behavior
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddAzureMonitorLogExporter(options => options.ConnectionString = Conf.ApplicationInsights.ConnectionString);

                // Include formatted log messages (recommended for better readability)
                options.IncludeFormattedMessage = true;

                // Include log scopes for additional context
                options.IncludeScopes = true;
            });

            // Set minimum log level to Information to capture important events
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Build the kernel with the configured logger factory
        var kernelBuilder = Kernel.CreateBuilder();

        // IMPORTANT: Register the logger factory as a singleton
        // This ensures all Semantic Kernel operations use the configured logging
        kernelBuilder.Services.AddSingleton(loggerFactory);

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: Conf.AzureOpenAI.DeploymentName,
            endpoint: Conf.AzureOpenAI.Endpoint,
            apiKey: Conf.AzureOpenAI.ApiKey);

        var kernel = kernelBuilder.Build();

        Console.WriteLine("Sending request to AI model...");
        Console.WriteLine("Telemetry data will be exported to Application Insights.\n");

        // Execute a simple prompt - this will generate telemetry data
        var answer = await kernel.InvokePromptAsync("Why is the sky blue in one sentence?");

        Console.WriteLine($"Answer: {answer}\n");

        // IMPORTANT: Reset the switch after use in production scenarios
        AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", false);
    }


}
