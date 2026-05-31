using A2A;
using A2A.AspNetCore;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using SharedConfig;
using System.ClientModel;

Conf.Init<Program>();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();

var invoiceQueryPlugin = new InvoiceQuery();

var hostA2AAgent = new AzureOpenAIClient(
           new Uri(Conf.AzureFoundry.Endpoint),
           new ApiKeyCredential(Conf.AzureFoundry.ApiKey))
       .GetChatClient(Conf.AzureFoundry.DeploymentName)
       .AsAIAgent(
          "You specialize in handling queries related to invoices.",
          "InvoiceAgent",
          tools: [
            AIFunctionFactory.Create(invoiceQueryPlugin.QueryInvoices),
            AIFunctionFactory.Create(invoiceQueryPlugin.QueryByTransactionId),
            AIFunctionFactory.Create(invoiceQueryPlugin.QueryByInvoiceId)
          ]);

AgentCard hostA2AAgentCard = GetInvoiceAgentCard(builder.Configuration["urls"].Split(';'));

// Agent-to-Agent (A2A) communication setup
builder.AddA2AServer(hostA2AAgent);

var app = builder.Build();

// Agent-to-Agent (A2A) communication endpoints
app.MapA2AHttpJson(hostA2AAgent, "/");
app.MapA2AJsonRpc(hostA2AAgent, "/");
app.MapWellKnownAgentCard(hostA2AAgentCard);

await app.RunAsync();


static AgentCard GetInvoiceAgentCard(string[] agentUrls)
{
    var capabilities = new AgentCapabilities()
    {
        Streaming = false,
        PushNotifications = false,
    };

    var invoiceQuery = new A2A.AgentSkill()
    {
        Id = "id_invoice_agent",
        Name = "InvoiceQuery",
        Description = "Handles requests relating to invoices.",
        Tags = ["invoice", "semantic-kernel"],
        Examples =
        [
            "List the latest invoices for Contoso.",
        ],
    };

    return new()
    {
        Name = "InvoiceAgent",
        Description = "Handles requests relating to invoices.",
        Version = "1.0.0",
        DefaultInputModes = ["text"],
        DefaultOutputModes = ["text"],
        Capabilities = capabilities,
        Skills = [invoiceQuery],
        SupportedInterfaces = CreateAgentInterfaces(agentUrls)
    };
}

static List<AgentInterface> CreateAgentInterfaces(string[] agentUrls)
{
    List<AgentInterface> agentInterfaces = [];

    agentInterfaces.AddRange(agentUrls.Select(url => new AgentInterface
    {
        Url = url,
        ProtocolBinding = ProtocolBindingNames.JsonRpc,
        ProtocolVersion = "1.0",
    }));

    agentInterfaces.AddRange(agentUrls.Select(url => new AgentInterface
    {
        Url = url,
        ProtocolBinding = ProtocolBindingNames.HttpJson,
        ProtocolVersion = "1.0",
    }));

    return agentInterfaces;
}

