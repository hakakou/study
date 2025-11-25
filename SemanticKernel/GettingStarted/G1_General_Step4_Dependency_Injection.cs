using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

public class G1_General_Step4_Dependency_Injection : ITest
{
    public async Task Run()
    {
        var collection = new ServiceCollection();

        collection.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        collection.AddOpenAIChatClient(
            modelId: "gpt-5-nano-2025-08-07",
            apiKey: Conf.OpenAI.ApiKey);

        collection.AddOpenAIChatCompletion(
            modelId: "gpt-5-nano-2025-08-07",
            apiKey: Conf.OpenAI.ApiKey);

        var kernelBuilder = collection.AddKernel();
        kernelBuilder.Plugins.AddFromType<TimeInformation>();

        // Build service provider and get kernel
        var provider = collection.BuildServiceProvider();
        var kernel = provider.GetRequiredService<Kernel>();

        PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        // Invoke the kernel with a templated prompt and stream the results to the display
        KernelArguments arguments = new(settings)
            { { "topic", "Athens, Greece?" } };
        
        await foreach (var update in
                  kernel.InvokePromptStreamingAsync(
                  "What time is  it in {{$topic}}?", arguments))
        {
            Console.Write(update);
        }
    }

    public class TimeInformation(ILogger<TimeInformation> logger)
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime()
        {
            var utcNow = DateTime.UtcNow.ToString("R");
            logger.LogInformation("Returning current time {0}", utcNow);
            return utcNow;
        }
    }
}
