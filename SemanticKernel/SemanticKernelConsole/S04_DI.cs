using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

// https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel?pivots=programming-language-csharp
// The kernel is extremely lightweight (since it's just a container for services and plugins),
// so creating a new kernel for each use is not a performance concern.
public class S04_DI : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var services = new ServiceCollection();

        // services.AddOpenAIChatCompletion( ..

        // Create singletons of your plugins
        services.AddSingleton<SpeakerPlugin>((s) => new SpeakerPlugin());

        services.AddOpenAIChatCompletion("gpt-4o-mini",
                apiKey: Conf.OpenAI.ApiKey);

        // Create the plugin collection (using the KernelPluginFactory to create plugins from objects)
        services.AddSingleton<KernelPluginCollection>((serviceProvider) =>
        {
            return [
                KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<SpeakerPlugin>()),
            ];
        });

        // Finally, create the Kernel service with the service provider and plugin collection
        services.AddTransient((serviceProvider) =>
        {
            KernelPluginCollection pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();
            return new Kernel(serviceProvider, pluginCollection);
        });

        sp = services.BuildServiceProvider();
        var kernel = sp.GetRequiredService<Kernel>();

        ChatHistory history = [];
        history.AddUserMessage("tell me a joke with a dog theme");
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Streaming
        var response = chatCompletionService.GetStreamingChatMessageContentsAsync(history, 
            executionSettings: openAIPromptExecutionSettings, kernel: kernel);

        await foreach (var result in response)
        {
            Console.Write(result);
        }
    }

    public class SpeakerPlugin
    {
        [KernelFunction, Description("Tells a joke with specific theme")]
        public string TellJoke(string theme)
        {
            return "A dog is as good as it's owner.";
        }
    }

    private ServiceProvider sp;
}