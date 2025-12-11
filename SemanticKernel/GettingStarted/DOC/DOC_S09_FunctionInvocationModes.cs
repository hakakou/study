using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace DOC_S09_FunctionInvocationModes;

public class DOC_S09_FunctionInvocationModes : ITest
{
    public async Task Run()
    {
        //await DemonstrateAutoFunctionInvocation();
        //await DemonstrateConcurrentFunctionInvocation();
        //await DemonstrateManualFunctionInvocation();
        await DemonstrateManualFunctionInvocationWithStreaming();
    }

    /// <summary>
    /// Demonstrates auto function invocation mode (default behavior).
    /// The AI model decides which functions to call and Semantic Kernel automatically invokes them.
    /// Function results are automatically added to chat history and sent back to the model.
    /// </summary>
    private async Task DemonstrateAutoFunctionInvocation()
    {
        Utils.PrintSectionHeader("Auto Function Invocation (Default Behavior)");

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey);
        builder.Plugins.AddFromType<WeatherPlugin>();
        builder.Plugins.AddFromType<TimePlugin>();

        Kernel kernel = builder.Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Auto function invocation is the default behavior.
        // Functions are automatically invoked by Semantic Kernel when chosen by the AI model.
        // If you want to be explicit about this behavior, you can use:
        // PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true) };
        PromptExecutionSettings settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var history = new ChatHistory();
        history.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

        var result = await chatCompletionService.GetChatMessageContentAsync(history, settings, kernel);

        history.Add(result);
        history.PrintChatHistory();
    }

    /// <summary>
    /// Demonstrates concurrent function invocation.
    /// When the AI model chooses multiple functions (parallel function calling),
    /// they can be invoked concurrently instead of sequentially for better performance.
    /// </summary>
    private async Task DemonstrateConcurrentFunctionInvocation()
    {
        Utils.PrintSectionHeader("Concurrent Function Invocation");

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey);
        builder.Plugins.AddFromType<NewsPlugin>();
        builder.Plugins.AddFromType<TimePlugin>();

        Kernel kernel = builder.Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        FunctionChoiceBehaviorOptions options = new()
        {
            AllowConcurrentInvocation = true,
            AllowParallelCalls = true
        };

        PromptExecutionSettings settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: options)
        };

        var history = new ChatHistory();
        history.AddUserMessage("Good morning! What is the current time and latest news headlines?");

        var result = await chatCompletionService.GetChatMessageContentAsync(history, settings, kernel);

        history.Add(result);
        history.PrintChatHistory();
    }

    /// <summary>
    /// Demonstrates manual function invocation mode.
    /// The caller has full control over which functions to invoke, how to invoke them,
    /// and how to handle exceptions. This is useful for custom error handling,
    /// logging, or conditional function execution.
    /// </summary>
    private async Task DemonstrateManualFunctionInvocation()
    {
        Utils.PrintSectionHeader("Manual Function Invocation");

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey);
        builder.Plugins.AddFromType<WeatherPlugin>();
        builder.Plugins.AddFromType<TimePlugin>();

        Kernel kernel = builder.Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Manual function invocation must be enabled explicitly by setting autoInvoke to false.
        // This gives the caller full control over the function invocation process.
        PromptExecutionSettings settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: false)
        };

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

        while (true)
        {
            // Get the AI model's response
            ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);

            // Check if the AI model has generated a final text response
            if (result.Content is not null)
            {
                Console.WriteLine(result.Content);
                break;
            }

            // Add the AI model's response (containing function calls) to chat history.
            // This is required by the models to preserve the conversation context.
            chatHistory.Add(result);

            // Extract function calls from the AI model's response
            IEnumerable<FunctionCallContent> functionCalls = FunctionCallContent.GetFunctionCalls(result);
            if (!functionCalls.Any())
            {
                break;
            }

            Console.WriteLine($"AI model requested {functionCalls.Count()} function(s) to be invoked:");

            // Manually iterate over each function call and invoke it
            // This gives you control over execution order, error handling, logging, etc.
            foreach (FunctionCallContent functionCall in functionCalls)
            {
                Console.WriteLine($"  - Invoking: {functionCall.PluginName}.{functionCall.FunctionName}");

                try
                {
                    // Invoke the function and get the result
                    FunctionResultContent resultContent = await functionCall.InvokeAsync(kernel);

                    // Add the function result to the chat history so the AI model can reason about it
                    chatHistory.Add(resultContent.ToChatMessage());

                    Console.WriteLine($"    Result: {resultContent.Result}");
                }
                catch (Exception ex)
                {
                    // Handle exceptions gracefully and add error details to chat history
                    // The AI model can reason about the error and potentially suggest alternatives
                    Console.WriteLine($"    Error: {ex.Message}");

                    // Option 1: Add the exception to chat history
                    chatHistory.Add(new FunctionResultContent(functionCall, ex).ToChatMessage());

                    // Option 2: Add a custom error message that the AI model can reason about
                    // chatHistory.Add(new FunctionResultContent(functionCall, "Error: Unable to retrieve weather data.").ToChatMessage());
                }
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Demonstrates manual function invocation with streaming chat completion.
    /// Function calls are streamed piece by piece, so we need to build them
    /// using FunctionCallContentBuilder before invoking them.
    /// </summary>
    private async Task DemonstrateManualFunctionInvocationWithStreaming()
    {
        Utils.PrintSectionHeader("Manual Function Invocation with Streaming");

        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey);
        builder.Plugins.AddFromType<WeatherPlugin>();
        builder.Plugins.AddFromType<TimePlugin>();

        Kernel kernel = builder.Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Enable manual function invocation for streaming API
        PromptExecutionSettings settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: false)
        };

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

        while (true)
        {
            AuthorRole? authorRole = null;

            // FunctionCallContentBuilder is used to build function calls from streaming content.
            // Since function calls are streamed, we need to collect all the pieces before invoking them.
            FunctionCallContentBuilder fccBuilder = new();

            // Stream the AI model's response
            await foreach (StreamingChatMessageContent streamingContent in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
            {
                // Check if the AI model has generated text content and stream it to the console
                if (streamingContent.Content is not null)
                {
                    Console.Write(streamingContent.Content);
                }

                // Capture the author role from the first streaming chunk
                authorRole ??= streamingContent.Role;

                // Collect function call details from each streaming chunk
                fccBuilder.Append(streamingContent);
            }

            Console.WriteLine();

            // Build the complete function calls from the streamed content
            IReadOnlyList<FunctionCallContent> functionCalls = fccBuilder.Build();
            if (!functionCalls.Any())
            {
                // No function calls found, exit the loop
                break;
            }

            // Create a chat message to preserve the function calls in the chat history
            ChatMessageContent fcContent = new ChatMessageContent(role: authorRole
                ?? default, content: null);
            chatHistory.Add(fcContent);

            Console.WriteLine($"AI model requested {functionCalls.Count} function(s) to be invoked:");

            // Iterate over function calls and invoke them
            // Note: This can be modified to invoke functions concurrently if needed
            foreach (FunctionCallContent functionCall in functionCalls)
            {
                // Add the original function call to the chat message content
                fcContent.Items.Add(functionCall);

                Console.WriteLine($"  - Invoking: {functionCall.PluginName}.{functionCall.FunctionName}");

                // Invoke the function
                FunctionResultContent functionResult = await functionCall.InvokeAsync(kernel);

                // Add the function result to the chat history
                chatHistory.Add(functionResult.ToChatMessage());

                Console.WriteLine($"    Result: {functionResult.Result}");
            }

            Console.WriteLine();
        }
    }
}

// Plugin that provides weather information
public class WeatherPlugin
{
    [KernelFunction("get_weather")]
    [Description("Gets the current weather for a city")]
    [return: Description("The current weather description")]
    public async Task<string> GetWeatherAsync(
        [Description("The city name")] string city)
    {
        // Simulated weather data
        var weather = city.ToLower() switch
        {
            "boston" => "Rainy with a chance of thunderstorms",
            "seattle" => "Cloudy and drizzling",
            "miami" => "Sunny and warm",
            _ => "Weather data not available"
        };

        await Task.Delay(100); // Simulate API call
        return weather;
    }
}

// Plugin that provides news headlines
public class NewsPlugin
{
    [KernelFunction("get_latest_news")]
    [Description("Gets the latest news headlines")]
    [return: Description("Latest news headlines")]
    public async Task<string> GetLatestNewsAsync()
    {
        await Task.Delay(100); // Simulate API call
        return "Breaking: AI technology continues to advance rapidly. Tech stocks reach new highs.";
    }
}

// Plugin that provides time information
public class TimePlugin
{
    [KernelFunction("get_current_time")]
    [Description("Gets the current UTC time")]
    [return: Description("The current time in UTC")]
    public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
}