using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel;

public class S01A_IChatClient : ITest
{
    public async Task Run()
    {
        IChatClient client = new AzureOpenAIClient(
            new Uri(Conf.AzureFoundry.Endpoint),
            new ApiKeyCredential(Conf.AzureFoundry.ApiKey))
            .GetChatClient(Conf.AzureFoundry.DeploymentName)
            .AsIChatClient();

        ChatClientAgent agent = client.AsAIAgent(instructions: "You are a helpful assistant.");

        Console.WriteLine(await agent.RunAsync("My name is Alice and I love hiking."));
    }
}