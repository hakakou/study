using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

#pragma warning disable SKEXP0001

public class DOC_S11_Filters : ITest
{
    public async Task Run()
    {
        //await FunctionInvocationFilterExample();
        //await PromptRenderFilterExample();
        //await AutoFunctionInvocationFilterExample();
        //await DualModeFilterExample();
        await ChatCompletionServiceWithFiltersExample();
    }

    /// <summary>
    /// Example 1: Function Invocation Filter
    /// This filter is executed EVERY TIME a KernelFunction is invoked (both prompt-based and method-based functions).
    /// Use cases: Logging, validation, caching, exception handling, result modification, retrying with alternative models.
    /// </summary>
    private async Task FunctionInvocationFilterExample()
    {
        Utils.PrintSectionHeader("=== Example 1: Function Invocation Filter ===\n");

        var builder = Kernel.CreateBuilder();

        // Add chat completion service
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: Conf.AzureFoundry.DeploymentName,
            endpoint: Conf.AzureFoundry.Endpoint,
            apiKey: Conf.AzureFoundry.ApiKey);

        // Add the logging filter using dependency injection
        builder.Services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();
        builder.Services.AddLogging(configure => configure.AddConsole());

        // Alternative: Add filter directly to kernel after building
        // kernel.FunctionInvocationFilters.Add(new LoggingFilter(loggerFactory.CreateLogger<LoggingFilter>()));

        var kernel = builder.Build();

        // Create a simple function from a prompt
        var function = kernel.CreateFunctionFromPrompt("What is the capital of {{$country}}?");

        // Invoke the function - the filter will automatically log before and after execution
        var result = await kernel.InvokeAsync(function, new() { ["country"] = "France" });

        Console.WriteLine($"Result: {result}\n");
        Console.WriteLine();
    }

    /// <summary>
    /// Example 2: Prompt Render Filter
    /// This filter is triggered ONLY during prompt rendering operations (functions created from prompts).
    /// Use cases: RAG (Retrieval-Augmented Generation), PII redaction, prompt modification, semantic caching.
    /// </summary>
    private async Task PromptRenderFilterExample()
    {
        Utils.PrintSectionHeader("=== Example 2: Prompt Render Filter ===\n");

        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: Conf.AzureFoundry.DeploymentName,
            endpoint: Conf.AzureFoundry.Endpoint,
            apiKey: Conf.AzureFoundry.ApiKey);

        // Add the prompt render filter
        builder.Services.AddSingleton<IPromptRenderFilter, SafePromptFilter>();

        var kernel = builder.Build();

        // Create a function from a prompt - this will trigger the prompt render filter
        var function = kernel.CreateFunctionFromPrompt("Tell me about {{$topic}}");

        Console.WriteLine("Invoking function with prompt render filter...");
        var result = await kernel.InvokeAsync(function, new() { ["topic"] = "artificial intelligence" });

        Console.WriteLine($"Result: {result}\n");
        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Auto Function Invocation Filter
    /// This filter operates ONLY within automatic function calling scenarios.
    /// Provides additional context: chat history, list of functions to execute, iteration counters.
    /// Use cases: Early termination, monitoring auto-invocation loops, controlling function execution flow.
    /// </summary>
    private async Task AutoFunctionInvocationFilterExample()
    {
        Utils.PrintSectionHeader("=== Example 3: Auto Function Invocation Filter ===\n");

        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: Conf.AzureFoundry.DeploymentName,
            endpoint: Conf.AzureFoundry.Endpoint,
            apiKey: Conf.AzureFoundry.ApiKey);

        // Add the auto function invocation filter
        builder.Services.AddSingleton<IAutoFunctionInvocationFilter, EarlyTerminationFilter>();

        var kernel = builder.Build();

        // Add a simple plugin with a function
        kernel.Plugins.AddFromType<WeatherPlugin>();

        // Create settings that enable auto function calling
        var settings = new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        Console.WriteLine("Invoking with auto function calling enabled...");
        var result = await kernel.InvokePromptAsync("What's the weather in Paris?", new(settings));

        Console.WriteLine($"Result: {result}\n");
        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: Dual Mode Filter (Streaming and Non-Streaming)
    /// Demonstrates how to handle both streaming and non-streaming invocations in a single filter.
    /// Important: Check context.IsStreaming to determine which mode is being used.
    /// </summary>
    private async Task DualModeFilterExample()
    {
        Utils.PrintSectionHeader("=== Example 4: Dual Mode Filter (Streaming & Non-Streaming) ===\n");

        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: Conf.AzureFoundry.DeploymentName,
            endpoint: Conf.AzureFoundry.Endpoint,
            apiKey: Conf.AzureFoundry.ApiKey);

        // Add the dual mode filter
        builder.Services.AddSingleton<IFunctionInvocationFilter, DualModeFilter>();

        var kernel = builder.Build();

        var function = kernel.CreateFunctionFromPrompt("Write a one-sentence greeting");

        // Test non-streaming mode
        Console.WriteLine("Non-streaming invocation:");
        var result = await kernel.InvokeAsync(function);
        Console.WriteLine($"Result: {result}\n");

        // Test streaming mode
        Console.WriteLine("Streaming invocation:");
        await foreach (var chunk in kernel.InvokeStreamingAsync<StreamingChatMessageContent>(function))
        {
            Console.Write(chunk.Content);
        }

        Console.WriteLine("\n");
    }

    /// <summary>
    /// Example 5: Using Filters with IChatCompletionService
    /// Demonstrates that filters only work with IChatCompletionService when a Kernel object is passed.
    /// IMPORTANT: Filters are attached to the Kernel, so the Kernel must be passed to trigger them.
    /// </summary>
    private async Task ChatCompletionServiceWithFiltersExample()
    {
        Utils.PrintSectionHeader("=== Example 5: Filters with IChatCompletionService ===\n");

        // Build kernel with chat completion service
        var builder = Kernel.CreateBuilder();
        
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: Conf.AzureFoundry.DeploymentName,
            endpoint: Conf.AzureFoundry.Endpoint,
            apiKey: Conf.AzureFoundry.ApiKey);

        var kernel = builder.Build();

        // Add filters to the kernel
        kernel.FunctionInvocationFilters.Add(new ChatCompletionLoggingFilter());

        // Get the IChatCompletionService from the kernel
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Create chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a helpful assistant.");
        chatHistory.AddUserMessage("What is the capital of France?");

        var executionSettings = new PromptExecutionSettings();

        Console.WriteLine("--- Scenario 1: WITHOUT passing Kernel (filters will NOT be triggered) ---");
        // When Kernel is NOT passed, filters will NOT be invoked
        var resultWithoutKernel = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory, 
            executionSettings);
        Console.WriteLine($"Result: {resultWithoutKernel.Content}\n");

        Console.WriteLine("--- Scenario 2: WITH passing Kernel (filters WILL be triggered) ---");
        // IMPORTANT: Passing Kernel here is REQUIRED to trigger filters
        // Filters are attached to the Kernel instance, not the service
        var resultWithKernel = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory, 
            executionSettings, 
            kernel); // <-- Kernel parameter triggers filters!
        Console.WriteLine($"Result: {resultWithKernel.Content}\n");

        Console.WriteLine();
    }
}

#region Filter Implementations

/// <summary>
/// Function Invocation Filter - Logs function execution before and after invocation.
/// This demonstrates the most common filter type for observability and monitoring.
/// CRITICAL: Must call 'await next(context)' to proceed with the function execution.
/// </summary>
public sealed class LoggingFilter : IFunctionInvocationFilter
{
    private readonly ILogger _logger;

    public LoggingFilter(ILogger<LoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnFunctionInvocationAsync(
        Microsoft.SemanticKernel.FunctionInvocationContext context,
        Func<Microsoft.SemanticKernel.FunctionInvocationContext, Task> next)
    {
        // Execute BEFORE the function is invoked
        _logger.LogInformation("FunctionInvoking - {PluginName}.{FunctionName}",
            context.Function.PluginName,
            context.Function.Name);

        Console.WriteLine($"[FILTER] About to invoke: {context.Function.PluginName}.{context.Function.Name}");

        // Access function arguments if needed
        foreach (var arg in context.Arguments)
        {
            Console.WriteLine($"[FILTER] Argument: {arg.Key} = {arg.Value}");
        }

        // IMPORTANT: Call next() to proceed to the next filter or the actual function
        // Without this call, the function will NOT be executed!
        await next(context);

        // Execute AFTER the function has been invoked
        _logger.LogInformation("FunctionInvoked - {PluginName}.{FunctionName}",
            context.Function.PluginName,
            context.Function.Name);

        Console.WriteLine($"[FILTER] Function completed: {context.Function.PluginName}.{context.Function.Name}");

        // You can modify the result here if needed
        // context.Result = new FunctionResult(context.Result, modifiedValue);
    }
}

/// <summary>
/// Prompt Render Filter - Modifies or validates prompts before they are sent to the AI.
/// Use cases: 
/// - Injecting additional context (RAG - Retrieval-Augmented Generation)
/// - Redacting PII (Personally Identifiable Information)
/// - Implementing semantic caching by overriding the result
/// - Validating prompts against responsible AI policies
/// </summary>
public sealed class SafePromptFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        // BEFORE rendering: Access function information
        var functionName = context.Function.Name;
        Console.WriteLine($"[PROMPT FILTER] Rendering prompt for function: {functionName}");

        // IMPORTANT: Call next() to perform the actual prompt rendering
        await next(context);

        // AFTER rendering: Access and optionally modify the rendered prompt
        Console.WriteLine($"[PROMPT FILTER] Original prompt: {context.RenderedPrompt}");

        // Example 1: Add a safety instruction to all prompts (Responsible AI)
        context.RenderedPrompt = $"{context.RenderedPrompt}\n\nIMPORTANT: Provide accurate and helpful information only.";

        // Example 2: Redact potential PII (this is a simple example, real PII detection would be more sophisticated)
        // context.RenderedPrompt = RedactPII(context.RenderedPrompt);

        // Example 3: Implement semantic caching - if prompt matches a cached prompt, override the result
        // if (TryGetCachedResult(context.RenderedPrompt, out var cachedResult))
        // {
        //     context.Result = cachedResult;
        // }

        Console.WriteLine($"[PROMPT FILTER] Modified prompt: {context.RenderedPrompt}");
    }
}

/// <summary>
/// Auto Function Invocation Filter - Controls the automatic function calling process.
/// Provides rich context including:
/// - Chat history
/// - List of all functions to be executed
/// - Iteration counters
/// Can terminate the auto-invocation loop early if desired results are obtained.
/// </summary>
public sealed class EarlyTerminationFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"[AUTO FILTER] Function call iteration: {context.FunctionCount}");
        Console.WriteLine($"[AUTO FILTER] Executing function: {context.Function.Name}");

        // Access additional context available in auto function invocation
        // - context.ChatHistory: The conversation history
        // - context.FunctionCount: How many functions have been called in this iteration
        // - context.RequestSequenceIndex: The index of this request in the sequence

        // IMPORTANT: Call next() to execute the function
        await next(context);

        // Get the function result
        var result = context.Result.GetValue<string>();
        Console.WriteLine($"[AUTO FILTER] Function result: {result}");

        // Example: Terminate early if we got a satisfactory result
        // This prevents unnecessary additional function calls
        if (!string.IsNullOrEmpty(result) && result.Contains("Paris"))
        {
            Console.WriteLine("[AUTO FILTER] Desired result obtained, terminating auto-invocation loop");
            context.Terminate = true; // Stop the auto function calling process
        }

        // Other use cases for termination:
        // - Maximum iteration count reached
        // - Specific condition met
        // - Error or malicious content detected
    }
}

/// <summary>
/// Dual Mode Filter - Handles both streaming and non-streaming function invocations.
/// CRITICAL: Use context.IsStreaming to determine the invocation mode.
/// - Streaming mode: Return IAsyncEnumerable<T>
/// - Non-streaming mode: Return T
/// </summary>
public sealed class DualModeFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        Microsoft.SemanticKernel.FunctionInvocationContext context,
        Func<Microsoft.SemanticKernel.FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"[DUAL FILTER] Invocation mode: {(context.IsStreaming ? "Streaming" : "Non-Streaming")}");

        // Call next filter in pipeline or actual function
        await next(context);

        // Check which function invocation mode is used
        if (context.IsStreaming)
        {
            // STREAMING MODE: Result must be IAsyncEnumerable<T>
            var enumerable = context.Result.GetValue<IAsyncEnumerable<StreamingChatMessageContent>>();
            context.Result = new FunctionResult(context.Result, OverrideStreamingDataAsync(enumerable!));
        }
        else
        {
            // NON-STREAMING MODE: Result can be the direct value
            var data = context.Result.GetValue<string>();
            context.Result = new FunctionResult(context.Result, OverrideNonStreamingData(data!));
        }
    }

    /// <summary>
    /// Modifies streaming data by appending a suffix to each chunk.
    /// Uses 'yield return' to maintain streaming behavior.
    /// </summary>
    private async IAsyncEnumerable<StreamingChatMessageContent> OverrideStreamingDataAsync(
        IAsyncEnumerable<StreamingChatMessageContent> data)
    {
        await foreach (var item in data)
        {
            // Modify each streamed chunk
            yield return new StreamingChatMessageContent(
                item.Role,
                item.Content + " [filtered]",
                item.InnerContent,
                item.ChoiceIndex,
                item.ModelId);
        }
    }

    /// <summary>
    /// Modifies non-streaming data by appending a suffix to the complete result.
    /// </summary>
    private string OverrideNonStreamingData(string data)
    {
        return $"{data} [filtered in non-streaming mode]";
    }
}

/// <summary>
/// Simple filter for demonstrating IChatCompletionService filter behavior.
/// This filter will only be invoked when the Kernel is passed to the chat completion service.
/// </summary>
public sealed class ChatCompletionLoggingFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        Microsoft.SemanticKernel.FunctionInvocationContext context,
        Func<Microsoft.SemanticKernel.FunctionInvocationContext, Task> next)
    {
        Console.WriteLine("[CHAT FILTER] ✓ Filter was triggered! Function invocation detected.");
        Console.WriteLine($"[CHAT FILTER] Function: {context.Function.Name}");

        await next(context);

        Console.WriteLine("[CHAT FILTER] ✓ Filter completed. Function execution finished.\n");
    }
}

#endregion

#region Helper Plugin

/// <summary>
/// Simple weather plugin for demonstrating auto function invocation.
/// </summary>
public class WeatherPlugin
{
    [KernelFunction("get_weather")]
    [System.ComponentModel.Description("Gets the weather for a specified city")]
    public string GetWeather(string city)
    {
        // Simulated weather data
        return $"The weather in {city} is sunny and 22°C";
    }
}

#endregion
