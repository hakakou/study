using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using System.ClientModel;
using System.Collections.ObjectModel;

public class S208_OpenAIAssistantAgent : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        //var kernel = builder.Build();

        // Package Microsoft.SemanticKernel.Yaml
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

        //ChatCompletionAgent agent = new ChatCompletionAgent(templateConfig)
        //{
        //    Kernel = builder.Build(),
        //};

        //ChatHistory chat = [];
        //var res = agent.InvokeAsync(chat, new KernelArguments()
        //        {
        //            { "topic", "Dog" },
        //            { "length", "3" },
        //        });

        //await foreach (ChatMessageContent response in res)
        //{
        //    response.ConsoleOutputAgentChatMessage();
        //}

        // OpenAI specialization

        var clientProvider = OpenAIClientProvider.ForOpenAI(new ApiKeyCredential(Program.ApiKey));

        /*
        var clientProvider = OpenAIClientProvider.ForAzureOpenAI(new ApiKeyCredential(""), new Uri(""));
        // With Azure.Identity
        var clientProvider = OpenAIClientProvider.ForAzureOpenAI(new AzureCliCredential(), new Uri(""));
        */

        var openAIagent = await OpenAIAssistantAgent.CreateFromTemplateAsync(
            clientProvider: clientProvider,
            capabilities: new OpenAIAssistantCapabilities("gpt-4o-mini")
            {
                Metadata = AssistantSampleMetadata
            },
            kernel: builder.Build(),
            defaultArguments: new KernelArguments()
            {
                { "topic", "Dog" },
                { "length", "3" },
            },
            templateConfig);

        Console.WriteLine("OpenAI Assistant Agent: " + openAIagent.Id);

        string threadId = await openAIagent.CreateThreadAsync(
            new OpenAIThreadCreationOptions { Metadata = AssistantSampleMetadata });

        await foreach (ChatMessageContent response in openAIagent.InvokeAsync(threadId))
        {
            response.ConsoleOutputAgentChatMessage();
        }
    }

    protected static readonly ReadOnlyDictionary<string, string> AssistantSampleMetadata =
        new(new Dictionary<string, string>
        {
                { "hksample", bool.TrueString }
        });
}