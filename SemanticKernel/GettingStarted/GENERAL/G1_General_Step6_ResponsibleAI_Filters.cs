using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

public class G1_General_Step6_ResponsibleAI_Filters : ITest
{
    public async Task Run()
    {
        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o",
            apiKey: Conf.OpenAI.ApiKey);

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Information));
        builder.Services.AddSingleton<IPromptRenderFilter, MyPromptFilter>();
        builder.Services.AddSingleton<IPromptRenderFilter, ObservabilityPromptFilter>();

        var kernel = builder.Build();

        // Add filter without DI
        // kernel.PromptRenderFilters.Add(new ObservabilityPromptFilter(null));

        KernelArguments arguments = new() { { "card_number", "4444 3333 2222 1111" } };
        var result = await kernel.InvokePromptAsync(
            "Tell me useful information about credit card number {{$card_number}}?", arguments);

        Console.WriteLine(result);
    }

    public sealed class MyPromptFilter(ILogger<MyPromptFilter> logger) : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            if (context.Arguments.ContainsName("card_number"))
            {
                context.Arguments["card_number"] = "**** **** **** ****";
            }

            await next(context);

            context.RenderedPrompt += " NO RACISM";

            // Tell me useful information about credit card number **** **** **** ****? NO RACISM
        }
    }

    private sealed class ObservabilityPromptFilter(ILogger<ObservabilityPromptFilter> logger) : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            logger.LogInformation("Rendering prompt for {FunctionName}", context.Function.Name);
            // Rendering prompt for InvokePromptAsync_7ab8827a642446a2bc3a7b631a7f253c
            await next(context);
            logger.LogInformation("Rendered prompt: {RenderedPrompt}", context.RenderedPrompt);
        }
    }
}