using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

public class S03_MultiTurn : ITest
{
    public async Task Run()
    {
        var client = new AIProjectClient(
                endpoint: new Uri(Conf.MicrosoftFoundry2.Endpoint),
                tokenProvider: new AzureCliCredential());

        AIAgent agent = client
            .AsAIAgent(
                model: "gpt-4.1",
                name: "ConversationAgent",
                instructions: "You are a helpful assistant.");

        // Use a session to maintain conversation context so the agent remembers what was said earlier.
        AgentSession session = await agent.CreateSessionAsync();

        Console.WriteLine(await agent.RunAsync("My name is Alice and I love hiking.", session));
        Console.WriteLine(await agent.RunAsync("What do you remember about me?", session));
    }
}