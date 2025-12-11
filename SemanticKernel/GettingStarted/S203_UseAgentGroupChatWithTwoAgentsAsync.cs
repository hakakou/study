using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;


public class S203_UseAgentGroupChatWithTwoAgentsAsync(Kernel kernel, ILoggerFactory loggerFactory) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
             .DefaultChatCompletion();

        //services.AddLogging(c =>
        //    c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        ChatCompletionAgent agentReviewer =
            new()
            {
                Name = "Publisher",
                Instructions = """
        You are a publisher who understands which books would become best sellers.
        The goal is to give the writer feedback to make it a best seller.
        If it's ready, state that it is APPROVED  (in english) in capitals.
        If not, provide insight on how to refine suggested copy without example, 
        but don't write the word APPROVED.
        """,
                Kernel = kernel,
            };

        ChatCompletionAgent agentWriter =
            new()
            {
                Name = "Writer",
                Instructions = """
        You are a writer with ten years of writting engaging stories, inspired by Frank Herbert.
        The goal is to expand the idea as a story.
        Only provide a single proposal per response.
        Consider suggestions when refining an idea and rewrite the entire story if told so.
        """,
                Kernel = kernel,
            };

        AgentGroupChat agentChat = new(agentWriter, agentReviewer)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new ApprovalTerminationStrategy()
                {
                    Agents = [agentReviewer],
                    MaximumIterations = 20,
                },

                // Default SelectionStrategy is SequentialSelectionStrategy
            }
        };

        // Invoke chat and display messages.
        ChatMessageContent input = new(AuthorRole.User,
        """
Write a short story about demographic crisis in the future. 
Assume that the population is aging and the birth rate is low.
"""
        );
        //Write a short story about the future where programmers who can control AI are the social elite.
        //They are more philosphers than coders as their moral decisions shape the world.
        //Chapter titles should include verses from ancient greek philosophers.

        agentChat.AddChatMessage(input);
        input.PrintChatMessageContent();

        await foreach (ChatMessageContent response in agentChat.InvokeAsync())
        {
            response.PrintChatMessageContent();
        }

        Console.WriteLine($"\n[IS COMPLETED: {agentChat.IsComplete}]");
    }

    private sealed class ApprovalTerminationStrategy : TerminationStrategy
    {
        // Terminate when the final message contains the term "approve"
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent,
            IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)

            => Task.FromResult(history[history.Count - 1].Content?
                .Contains("APPROVED") ?? false);
    }
}