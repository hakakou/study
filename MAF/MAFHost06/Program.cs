// Run with: func start
// Then call: POST http://localhost:7071/api/agents/HostedAgent/run

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AzureFunctions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;
using SharedConfig;
using System.ClientModel;

Conf.Init<Program>();

var agent = new AzureOpenAIClient(
    new Uri(Conf.AzureFoundry.Endpoint),
    new ApiKeyCredential(Conf.AzureFoundry.ApiKey))
    .GetChatClient(Conf.AzureFoundry.DeploymentName)
    .AsAIAgent(
        instructions: "You are a helpful assistant hosted in Azure Functions.",
        name: "HostedAgent");

// Configure the function app to host the AI agent.
// This will automatically generate HTTP API endpoints for the agent.
using IHost app = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication()
    .ConfigureDurableAgents(options => options.AddAIAgent(agent, timeToLive: TimeSpan.FromHours(1)))
    .Build();
app.Run();
