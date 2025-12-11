using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using OpenAI.Assistants;
using System.Collections.ObjectModel;

#pragma warning disable SKEXP0110

public class DOC_S34_AssistantAgentTemplate(Kernel kernel, AssistantClient assistantClient) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultOpenAIAssistantClient();
        services.AddLogging(c =>
            c.AddConsole().SetMinimumLevel(LogLevel.Information));
    }

    public async Task Run()
    {
        PromptTemplateConfig templateConfig = KernelFunctionYaml.ToPromptTemplateConfig("""
            name: GenerateStory
            template: |
              Tell a story about {{$topic}} that is {{$length}} sentences long.
            template_format: semantic-kernel
            description: A function that generates a story about a topic.
            input_variables:
              - name: topic
                description: The topic of the story.
                is_required: true
              - name: length
                description: The number of sentences in the story.
                is_required: true
            output_variable:
              description: The generated story.
            execution_settings:
              default:
                temperature: 0.6
            """);

        // Instructions, Name and Description properties defined via the PromptTemplateConfig.
        Assistant assistant = await assistantClient.CreateAssistantFromTemplateAsync(
            Conf.AzureFoundry.DeploymentName, templateConfig, metadata: SampleMetadata);

        //Assistant assistant = await assistantClient.GetAssistantAsync("asst_5C4h4bfXnVn0n8woVDQe32xR");

        OpenAIAssistantAgent agent = new(
            assistant,
            assistantClient,
            templateFactory: new KernelPromptTemplateFactory(),
            templateFormat: PromptTemplateConfig.SemanticKernelTemplateFormat)
        {
            Arguments = new()
            {
                { "topic", "Dog" },
                { "length", "3" }
            }
        };

        // Create a thread for the agent conversation.
        // var thread = new OpenAIAssistantAgentThread(assistantClient, "thread_qxUd58BMbpoxPn9ko7qecBps");
        var thread = new OpenAIAssistantAgentThread(assistantClient, metadata: SampleMetadata);

        //await agent.InvokeAgentAsync(thread, "What was the last joke again? And how many times did I ask you?");

        await agent.InvokeAgentAsync(thread, null);

        // Invoke the agent with the override arguments.
        await agent.InvokeAgentAsync(thread, "Make the same story to use a donkey", new()
                {
                    { "topic", "Donkey" },
                    { "length", "3" },
                });

        Console.WriteLine("Conversation thread messages:");
        await foreach (var response in thread.GetMessagesAsync())
        {
            response.PrintChatMessageContent();
        }

        await thread.DeleteAsync();
        await assistantClient.DeleteAssistantAsync(agent.Id);
    }

    protected static readonly ReadOnlyDictionary<string, string> SampleMetadata =
    new(new Dictionary<string, string>
    {
            { "sksample", bool.TrueString }
    });

}
