using Microsoft.SemanticKernel;
using SharedConfig;

public class S209_OpenAIAssistantAgentFunctions : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        /*

        var clientProvider = OpenAIClientProvider.ForOpenAI(new ApiKeyCredential(Conf.OpenAI.ApiKey));

        var agent = await OpenAIAssistantAgent.CreateAsync(
            clientProvider,
            definition: new OpenAIAssistantDefinition("gpt-4o-mini")
            {
                Instructions = "Answer questions about the menu.",
                Name = "Menu Assistant",
            },
            kernel: kernel);

        agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());

        string threadId = await agent.CreateThreadAsync(
            new OpenAIThreadCreationOptions { });

        var message = new ChatMessageContent(AuthorRole.User, "What is the special drink and its price?");
        await agent.AddChatMessageAsync(threadId, message);
        message.ConsoleOutputAgentChatMessage();

        var responses = agent.InvokeAsync(threadId);
        await foreach (ChatMessageContent response in responses)
        {
            response.ConsoleOutputAgentChatMessage();
        }

        */
    }
}