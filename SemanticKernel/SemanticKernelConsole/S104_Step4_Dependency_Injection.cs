using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelConsole.Functions;
using System.ComponentModel;

public class S104_Step4_Dependency_Injection : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var collection = new ServiceCollection();
        collection.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        var kernelBuilder = collection.AddKernel();
        kernelBuilder.AddOpenAIChatCompletion("gpt-4o-mini",
            apiKey: Conf.OpenAI.ApiKey);

        kernelBuilder.Plugins.AddFromType<TimePlugin>();

        var provider = collection.BuildServiceProvider();

        var kernel = provider.GetRequiredService<Kernel>();

        var args = new KernelArguments { { "topic", "chocolate" } };

        await foreach (var k in kernel.InvokePromptStreamingAsync("Create a strange recipe for a {{$topic}} cake in JSON format", args))
        {
            Console.Write(k);
        }

    }
}