using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;

public class S01_Hello : ITest
{
    // https://learn.microsoft.com/en-us/agent-framework/overview/?pivots=programming-language-csharp
    public async Task Run()
    {
        // Microsoft.Agents.AI.OpenAI
        // var client = new ResponsesClient(new ApiKeyCredential(Conf.OpenAI.ApiKey));

        // az account clear 
        // az login
        // az login --tenant GUID

        var client = new AIProjectClient(
                endpoint: new Uri(Conf.MicrosoftFoundry2.Endpoint),
                tokenProvider: new AzureCliCredential());

        AIAgent agent = client
            .AsAIAgent(
                model: "gpt-4.1",
                name: "MyAgent",
                instructions: "You are a friendly assistant. Keep your answers brief.");

        var response = await agent.RunAsync("What is the largest city in France?");
        Console.WriteLine(response.Text);
        Console.WriteLine(response.Messages.Count);

        await foreach (var update in agent.RunStreamingAsync("Tell me a one-sentence fun fact."))
        {
            Console.Write(update);
        }
    }
}