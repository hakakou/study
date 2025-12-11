using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Memory;

public class DOC_S36_AgentMemoryWhiteboard(
    Kernel kernel,
    IChatClient chatCompletionService,
    IHttpClientFactory httpClientFactory) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatClient();
        services.DefaultMem0Provider();
    }

    public async Task Run()
    {
        Utils.PrintSectionHeader("Agent Memory: Whiteboard and Combined Strategies");

        Console.WriteLine("Whiteboard Memory retains short-term context about requirements,");
        Console.WriteLine("proposals, decisions, and actions from ongoing conversations.\n");
        Console.WriteLine("Combining Mem0 and Whiteboard provides both long-term and short-term memory.\n");

        await Example1_WhiteboardMemoryAsync();
        // await Example2_CombiningMem0AndWhiteboardAsync();
    }

    /// <summary>
    /// Demonstrates Whiteboard Memory for short-term context retention.
    /// Whiteboard captures requirements, proposals, decisions, and actions from conversations.
    /// </summary>
    private async Task Example1_WhiteboardMemoryAsync()
    {
        Utils.PrintSectionHeader("Example 1: Whiteboard Memory for Short-Term Context");

        Console.WriteLine("Whiteboard Memory captures key information (requirements, proposals, decisions, actions)");
        Console.WriteLine("from conversations and maintains context even when chat history is truncated.\n");

        // Create a whiteboard provider with custom options
        var whiteboardProvider = new WhiteboardProvider(chatCompletionService,
            options: new WhiteboardProviderOptions
            {
                MaxWhiteboardMessages = 10, // Maximum number of items to retain on the whiteboard
                ContextPrompt = "The following requirements, proposals, decisions, and actions have been captured from the conversation:",
                WhiteboardEmptyPrompt = "The whiteboard is currently empty - no items have been captured yet."
            });

        ChatCompletionAgent agent = new()
        {
            Name = "TravelAgent",
            Instructions = "You are a helpful travel planning assistant. Help users plan their trips by tracking their requirements and decisions.",
            Kernel = kernel,
        };

        // Create the agent thread and add the whiteboard provider
        ChatHistoryAgentThread agentThread = new();
        agentThread.AIContextProviders.Add(whiteboardProvider);

        // First message - establish a requirement
        Console.WriteLine("[Step 1] User expresses initial requirement:\n");

        await agent.InvokeAgentAsync(agentThread, "Hello! I want to plan a trip.");

        // Second message - add more details and decisions
        Console.WriteLine("\n[Step 2] User provides additional requirements:\n");
        await agent.InvokeAgentAsync(agentThread, "I prefer to travel in September and stay for 5 days. Budget is around $3000.");

        // Third message - make decisions
        Console.WriteLine("\n[Step 3] User makes decisions:\n");
        await agent.InvokeAgentAsync(agentThread, "I've decided to book the hotel near the Eiffel Tower. Can you also suggest some restaurants?");

        // Fourth message - verify context retention
        Console.WriteLine("\n[Step 4] Verify whiteboard retains context:\n");
        await agent.InvokeAgentAsync(agentThread, "What are my current travel plans?");
        // Agent should recall all the requirements and decisions from the whiteboard
        await agentThread.DeleteAsync();

        Console.WriteLine("\n" + new string('-', 80));
    }

    /// <summary>
    /// Demonstrates combining Mem0 (long-term) and Whiteboard (short-term) memory.
    /// This provides both persistent user preferences and context-aware conversation tracking.
    /// </summary>
    private async Task Example2_CombiningMem0AndWhiteboardAsync()
    {
        Utils.PrintSectionHeader("Example 2: Combining Mem0 and Whiteboard Memory");

        Console.WriteLine("Combining both memory types provides:");
        Console.WriteLine("- Mem0: Long-term user preferences and history");
        Console.WriteLine("- Whiteboard: Short-term conversation context and decisions\n");

        var httpClient = httpClientFactory.CreateClient("mem0");

        // Create Mem0 provider for long-term memory
        var mem0Provider = new Mem0Provider(httpClient, options: new Mem0ProviderOptions
        {
            UserId = "U5",
            ApplicationId = "CombinedMemoryDemo",
        });

        // Create Whiteboard provider for short-term context
        var whiteboardProvider = new WhiteboardProvider(chatCompletionService, options: new WhiteboardProviderOptions
        {
            MaxWhiteboardMessages = 8,
            ContextPrompt = "Current conversation context (requirements, decisions, actions):"
        });

        ChatCompletionAgent agent = new()
        {
            Name = "PersonalAssistant",
            Instructions = "You are a personal assistant that combines long-term user knowledge with short-term conversation context. " +
                          "Use memories to personalize responses and whiteboard to track current goals.",
            Kernel = kernel,
        };

        // Add both providers to the agent thread
        ChatHistoryAgentThread agentThread = new();
        agentThread.AIContextProviders.Add(mem0Provider);
        agentThread.AIContextProviders.Add(whiteboardProvider);

        try
        {
            // First: Establish long-term preferences (stored in Mem0)
            Console.WriteLine("[Step 1] Establishing long-term preferences:\n");
            await agent.InvokeAgentAsync(agentThread, 
                "I prefer morning meetings, I work in the tech industry, and I'm based in Seattle.");

            // Second: Start a specific task (tracked on Whiteboard)
            Console.WriteLine("\n[Step 2] Starting a specific task:\n");
            await agent.InvokeAgentAsync(agentThread,
                "I need to prepare for an important product launch meeting next week. " +
                "I want to create a presentation and review the marketing materials.");

            // Third: Make decisions about the task
            Console.WriteLine("\n[Step 3] Making decisions:\n");
            await agent.InvokeAgentAsync(agentThread,
                "I've decided to schedule the presentation review for Tuesday morning at 9 AM. " +
                "I'll need to invite the design and marketing teams.");

            // Fourth: Query both memory types
            Console.WriteLine("\n[Step 4] Agent uses both long-term and short-term memory:\n");
            await agent.InvokeAgentAsync(agentThread,
                "Summarize my current tasks and consider my preferences.");
            // Response should incorporate both:
            // - Long-term: Morning preference, Seattle location, tech industry
            // - Short-term: Product launch meeting, Tuesday 9 AM, teams to invite
        }
        finally
        {
            await agentThread.DeleteAsync();
        }

        Console.WriteLine("\n" + new string('-', 80));
    }
}