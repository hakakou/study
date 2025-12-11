using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

public class G1_General_Step7_Observability_With_Filters : ITest
{
    public async Task Run()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "gpt-4o",
            apiKey: Conf.OpenAI.ApiKey);

        kernelBuilder.Plugins.AddFromType<TimeInformation>();

        kernelBuilder.Services.AddLogging(loggingBuilder =>
            loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Information));
        kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, MyFunctionFilter>();

        //kernelBuilder.Services.AddSingleton<IPromptRenderFilter, MyPromptFilter>();
        Kernel kernel = kernelBuilder.Build();

        // Add filter without DI
        kernel.PromptRenderFilters.Add(new MyPromptFilter());

        OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        Console.WriteLine(await kernel.InvokePromptAsync(
            "How many days until Christmas? Explain your thinking.", new(settings)));
    }

    private sealed class TimeInformation
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }

    // Function filter for observability.
    private sealed class MyFunctionFilter(ILogger<MyFunctionFilter> logger) : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            logger.LogInformation("Invoking {FunctionName}", context.Function.Name);

            await next(context);

            var metadata = context.Result?.Metadata;

            if (metadata is not null && metadata.ContainsKey("Usage"))
            {
                logger.LogInformation("Token usage: {Usage}", metadata["Usage"]?.AsJson());
            }
        }
    }

    // Prompt filter for observability.
    private sealed class MyPromptFilter : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            Console.WriteLine($"Rendering prompt for {context.Function.Name}");

            await next(context);

            Console.WriteLine($"Rendered prompt: {context.RenderedPrompt}");
        }
    }
}