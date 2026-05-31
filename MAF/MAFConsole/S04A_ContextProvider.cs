using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel;
using System.Text;
using System.Text.Json;

public class S04A_ContextProvider : ITest
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
                Instructions = "You are a friendly assistant. Always address the user by their name."
            },
            AIContextProviders = [new UserInfoMemory(client.AsIChatClient())]
        });

        // Create a new session for the conversation.
        AgentSession session = await agent.CreateSessionAsync();

        Console.WriteLine(">> Use session with blank memory\n");

        // Invoke the agent and output the text result.
        Console.WriteLine(await agent.RunAsync("Hello, what is the square root of 9?", session));
        Console.WriteLine(await agent.RunAsync("My name is Anna", session));
        Console.WriteLine(await agent.RunAsync("I am 20 years old", session));

        // We can serialize the session. The serialized state will include the state of the memory component.
        JsonElement sessionElement = await agent.SerializeSessionAsync(session);

        Console.WriteLine("\n>> Use deserialized session with previously created memories\n");

        // Later we can deserialize the session and continue the conversation with the previous memory component state.
        var deserializedSession = await agent.DeserializeSessionAsync(sessionElement);
        Console.WriteLine(await agent.RunAsync("What is my name and age?", deserializedSession));

        Console.WriteLine("\n>> Read memories using memory component\n");

        // It's possible to access the memory component via the agent's GetService method.
        var userInfo = agent.GetService<UserInfoMemory>()?.GetUserInfo(deserializedSession);

        // Output the user info that was captured by the memory component.
        Console.WriteLine($"MEMORY - User Name: {userInfo?.UserName}");
        Console.WriteLine($"MEMORY - User Age: {userInfo?.UserAge}");

        Console.WriteLine("\n>> Use new session with previously created memories\n");

        // It is also possible to set the memories using a memory component on an individual session.
        // This is useful if we want to start a new session, but have it share the same memories as a previous session.
        var newSession = await agent.CreateSessionAsync();
        if (userInfo is not null && agent.GetService<UserInfoMemory>() is UserInfoMemory newSessionMemory)
        {
            newSessionMemory.SetUserInfo(newSession, userInfo);
        }

        // Invoke the agent and output the text result.
        // This time the agent should remember the user's name and use it in the response.
        Console.WriteLine(await agent.RunAsync("What is my name and age?", newSession));
    }

    internal sealed class UserInfoMemory : AIContextProvider
    {
        private readonly ProviderSessionState<UserInfo> _sessionState;
        private IReadOnlyList<string>? _stateKeys;
        private readonly IChatClient _chatClient;

        public UserInfoMemory(IChatClient chatClient, Func<AgentSession?, UserInfo>? stateInitializer = null)
        {
            this._sessionState = new ProviderSessionState<UserInfo>(
                stateInitializer ?? (agentSession => new UserInfo()),
                stateKey: this.GetType().Name);
            this._chatClient = chatClient;
        }

        public override IReadOnlyList<string> StateKeys =>
            this._stateKeys ??= [this._sessionState.StateKey];

        public UserInfo GetUserInfo(AgentSession session)
            => this._sessionState.GetOrInitializeState(session);

        public void SetUserInfo(AgentSession session, UserInfo userInfo)
            => this._sessionState.SaveState(session, userInfo);

        protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = default)
        {
            var userInfo = this._sessionState.GetOrInitializeState(context.Session);

            StringBuilder instructions = new();

            // If we don't already know the user's name and age, add instructions to ask for them, otherwise just provide what we have to the context.
            instructions
                .AppendLine(
                    userInfo.UserName is null ?
                        "Ask the user for their name and politely decline to answer any questions until they provide it." :
                        $"The user's name is {userInfo.UserName}.")
                .AppendLine(
                    userInfo.UserAge is null ?
                        "Ask the user for their age and politely decline to answer any questions until they provide it." :
                        $"The user's age is {userInfo.UserAge}.");

            return new ValueTask<AIContext>(new AIContext
            {
                Instructions = instructions.ToString()
            });
        }

        protected override async ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
        {
            var userInfo = this._sessionState.GetOrInitializeState(context.Session);

            // H: I think this code should go in the ProvideAIContextAsync method,
            // before the response is generated, so that the agent can ask for the missing
            // information in the same turn if it's not already present in the memory.

            if ((userInfo.UserName is null || userInfo.UserAge is null)
                && context.RequestMessages.Any(x => x.Role == ChatRole.User))
            {
                var result = await this._chatClient.GetResponseAsync<UserInfo>(
                    context.RequestMessages,
                    new ChatOptions()
                    {
                        Instructions = "Extract the user's name and age from the message if present. If not present return nulls."
                    },
                    cancellationToken: cancellationToken);

                userInfo.UserName ??= result.Result.UserName;
                userInfo.UserAge ??= result.Result.UserAge;
            }

            this._sessionState.SaveState(context.Session, userInfo);
        }

    }

    internal sealed class UserInfo
    {
        public string? UserName { get; set; }
        public int? UserAge { get; set; }
    }
}