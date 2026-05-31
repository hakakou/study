using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.Json;

[RunDirectly]
public class S04B_TriageContextProvider : ITest
{
    public async Task Run()
    {
        var client = new AzureOpenAIClient(
            new Uri(Conf.AzureFoundry.Endpoint),
            new ApiKeyCredential(Conf.AzureFoundry.ApiKey))
            .GetChatClient(Conf.AzureFoundry.DeploymentName);


        AIAgent agent = client.AsAIAgent(new ChatClientAgentOptions()
        {
            ChatOptions = new()
            {
                Instructions = "You are an IT helpdesk triage assistant. " +
                               "Only offer to file a ticket once you have all required information."
            },
            AIContextProviders = [new TriageMemory(client.AsIChatClient())]
        });

        AgentSession session = await agent.CreateSessionAsync();

        await agent.WriteRunAsync("File a ticket for me. The issue is critical", session);
        await agent.WriteRunAsync("The email server is down.", session);
        await agent.WriteRunAsync("It's pretty critical, severity 1.", session);
        //await agent.WriteRunAsync("Yes I already rebooted my machine.", session); 
        await agent.WriteRunAsync("no", session);

        // Inspect captured state
        var ticket = agent.GetService<TriageMemory>()?.GetTicketInfo(session);
        Console.WriteLine($"MEMORY - System: {ticket?.AffectedSystem}");
        Console.WriteLine($"MEMORY - Severity: {ticket?.Severity}");
        Console.WriteLine($"MEMORY - Restarted: {ticket?.RestartTried}");

        // Round-trip through serialization, then ask for the final disposition
        JsonElement json = await agent.SerializeSessionAsync(session);
        var restored = await agent.DeserializeSessionAsync(json);
        Console.WriteLine();
        await agent.WriteRunAsync("Now file the ticket and tell me its disposition.", restored);
    }

    internal sealed class TriageMemory : AIContextProvider
    {
        private readonly ProviderSessionState<TicketInfo> _sessionState;
        private IReadOnlyList<string>? _stateKeys;
        private readonly IChatClient _chatClient;

        public TriageMemory(IChatClient chatClient)
        {
            _sessionState = new ProviderSessionState<TicketInfo>(_ => new TicketInfo(), GetType().Name);
            _chatClient = chatClient;
        }

        public override IReadOnlyList<string> StateKeys => _stateKeys ??= [_sessionState.StateKey];

        public TicketInfo GetTicketInfo(AgentSession session) => _sessionState.GetOrInitializeState(session);

        protected override async ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken ct = default)
        {
            var info = _sessionState.GetOrInitializeState(context.Session);

            if ((info.AffectedSystem is null || info.Severity is null || info.RestartTried is null))
            {
                // H: It only sees the current turn's messages, not the running history.
                // How to send history? 
                var resp = await _chatClient.GetResponseAsync<TicketInfo>(
                    context.AIContext.Messages,
                    new ChatOptions()
                    {
                        Instructions = "Extract the affected system, severity (1-4), and whether a restart was tried from the user's conversation. " +
                                       "Return null for anything not specified."
                    },
                    cancellationToken: ct);

                info.AffectedSystem ??= resp.Result.AffectedSystem;
                info.Severity ??= resp.Result.Severity;
                info.RestartTried ??= resp.Result.RestartTried;
            }

            _sessionState.SaveState(context.Session, info);

            var sb = new StringBuilder();

            sb.AppendLine(info.AffectedSystem is null
                ? "Ask the user for the affected system. Decline to answer other questions if not provided."
                : $"Affected System: {info.AffectedSystem}");

            sb.AppendLine(info.Severity is null
                ? "Ask the user to provide the severity (1-4, where 1 is critical). Decline to answer other questions if not provided."
                : $"Severity: {info.Severity}");

            sb.Append(info.RestartTried is null
                ? "Ask the user if they have already tried a restart (yes/no). Decline to answer other questions if it's not clear whether a reboot was done or not."
                : info.RestartTried.ToString());

            if (info.AffectedSystem != null && info.Severity.HasValue && info.RestartTried.HasValue)
            {
                string disposition = (info.Severity <= 2 && info.RestartTried == true) ? "ESCALATED" : "STANDARD";
                sb.AppendLine($"All information captured. Tell user we will file the ticket and mark as {disposition}.");
            }

            return new AIContext { Instructions = sb.ToString() };
        }

        protected override async ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken ct = default)
        {
            var spy = await context.Agent.SpySession(context.Session);
        }


    }

    internal sealed class TicketInfo
    {
        public string? AffectedSystem { get; set; }
        public int? Severity { get; set; }
        public bool? RestartTried { get; set; }
    }
}