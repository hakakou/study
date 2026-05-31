using A2A;
using A2A.AspNetCore;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using SharedConfig;
using System.ClientModel;
using System.ComponentModel;

string agentType = "INVOICE";

Conf.Init<Program>();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
string[] agentUrls = (builder.Configuration["urls"] ?? "http://localhost:60948").Split(';');

AIAgent CreateAgent(string? instructions = null, string? name = null, IList<AITool>? tools = null)
{
    return new AzureOpenAIClient(
               new Uri(Conf.AzureFoundry.Endpoint),
               new ApiKeyCredential(Conf.AzureFoundry.ApiKey))
           .GetChatClient(Conf.AzureFoundry.DeploymentName)
           .AsAIAgent(instructions, name, tools: tools);
}

var invoiceQueryPlugin = new InvoiceQuery();
IList<AITool> tools =
[
    AIFunctionFactory.Create(invoiceQueryPlugin.QueryInvoices),
    AIFunctionFactory.Create(invoiceQueryPlugin.QueryByTransactionId),
    AIFunctionFactory.Create(invoiceQueryPlugin.QueryByInvoiceId)
];

AIAgent hostA2AAgent;
AgentCard hostA2AAgentCard;

(hostA2AAgent, hostA2AAgentCard) = agentType.ToUpperInvariant() switch
{
    "INVOICE" => (
        CreateAgent(
            """
            You specialize in handling queries related to invoices.
            """, "InvoiceAgent", tools),
        GetInvoiceAgentCard(agentUrls)),

    "POLICY" => (
        CreateAgent(
            """
            You specialize in handling queries related to policies and customer communications.

            Always reply with exactly this text:

            Policy: Short Shipment Dispute Handling Policy V2.1

            Summary: "For short shipments reported by customers, first verify internal shipment records
            (SAP) and physical logistics scan data (BigQuery). If discrepancy is confirmed and logistics data
            shows fewer items packed than invoiced, issue a credit for the missing items. Document the
            resolution in SAP CRM and notify the customer via email within 2 business days, referencing the
            original invoice and the credit memo number. Use the 'Formal Credit Notification' email
            template."
            """, "PolicyAgent"),
        GetPolicyAgentCard(agentUrls)),

    "LOGISTICS" => (
        CreateAgent(
            """
            You specialize in handling queries related to logistics.

            Always reply with exactly:

            Shipment number: SHPMT-SAP-001
            Item: TSHIRT-RED-L
            Quantity: 900
            """, "LogisticsAgent"),
        GetLogisticsAgentCard(agentUrls)),

    _ => throw new ArgumentException($"Unsupported agent type: {agentType}"),
};

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

static AgentCard GetPolicyAgentCard(string[] agentUrls)
{
    var capabilities = new AgentCapabilities()
    {
        Streaming = false,
        PushNotifications = false,
    };

    var policyQuery = new A2A.AgentSkill()
    {
        Id = "id_policy_agent",
        Name = "PolicyAgent",
        Description = "Handles requests relating to policies and customer communications.",
        Tags = ["policy", "semantic-kernel"],
        Examples =
        [
            "What is the policy for short shipments?",
            ],
    };

    return new AgentCard()
    {
        Name = "PolicyAgent",
        Description = "Handles requests relating to policies and customer communications.",
        Version = "1.0.0",
        DefaultInputModes = ["text"],
        DefaultOutputModes = ["text"],
        Capabilities = capabilities,
        Skills = [policyQuery],
        SupportedInterfaces = CreateAgentInterfaces(agentUrls)
    };
}

static AgentCard GetLogisticsAgentCard(string[] agentUrls)
{
    var capabilities = new AgentCapabilities()
    {
        Streaming = false,
        PushNotifications = false,
    };

    var logisticsQuery = new A2A.AgentSkill()
    {
        Id = "id_logistics_agent",
        Name = "LogisticsQuery",
        Description = "Handles requests relating to logistics.",
        Tags = ["logistics", "semantic-kernel"],
        Examples =
        [
            "What is the status for SHPMT-SAP-001",
            ],
    };

    return new AgentCard()
    {
        Name = "LogisticsAgent",
        Description = "Handles requests relating to logistics.",
        Version = "1.0.0",
        DefaultInputModes = ["text"],
        DefaultOutputModes = ["text"],
        Capabilities = capabilities,
        Skills = [logisticsQuery],
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

