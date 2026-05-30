using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel;

[RunDirectly]
public class S06_IChatClient : ITest
{
    public async Task Run()
    {
        IChatClient client = new AzureOpenAIClient(
            new Uri(Conf.AzureFoundry.Endpoint),
            new ApiKeyCredential(Conf.AzureFoundry.ApiKey))
            .GetChatClient(Conf.AzureFoundry.DeploymentName)
            .AsIChatClient();

        ChatClientAgent agent = client.AsAIAgent(new ChatClientAgentOptions()
        {
            ChatOptions = new()
            {
                Instructions = "You are a helpful assistant."
            },
            ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
            {
                ChatReducer = new MessageCountingChatReducer(20),
            })
        });

        //var agent2 = new ChatClientAgent(client, instructions: "You are a helpful assistant");

        // Use a session to maintain conversation context so the agent remembers what was said earlier.
        AgentSession session = await agent.CreateSessionAsync();
        ChatClientAgentSession typedSession = (ChatClientAgentSession)session;
        Console.WriteLine(typedSession.ConversationId);

        Console.WriteLine(await agent.RunAsync("My name is Alice and I love hiking.", session));
        Console.WriteLine(await agent.RunAsync("What do you remember about me?", session));
    }
}