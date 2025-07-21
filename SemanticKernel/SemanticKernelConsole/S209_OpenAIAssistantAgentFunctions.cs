using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelConsole.Functions;
using System.ClientModel;

public class S209_OpenAIAssistantAgentFunctions : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var clientProvider = OpenAIClientProvider.ForOpenAI(new ApiKeyCredential(Program.ApiKey));

        var agent = await OpenAIAssistantAgent.CreateAsync(
            clientProvider,
            definition: new OpenAIAssistantDefinition("gpt-4o-mini")
            {
                Instructions = "Answer questions about the menu.",
                Name = "Menu Assistant",
            },
            kernel: builder.Build());

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
    }
}