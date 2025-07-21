using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;


#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class S204_UseKernelFunctionStrategiesWithAgentGroupChatAsync : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        string ReviewerName = "ArtDirector";
        string CopyWriterName = "CopyWriter";

        ILoggerFactory LoggerFactory  = builder.Build().Services.GetService<ILoggerFactory>();  

        ChatCompletionAgent agentReviewer =
            new()
            {
                Name = ReviewerName,
                Instructions = """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine if the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without examples.
        """,
                Kernel = builder.Build(),
                LoggerFactory = LoggerFactory,
            };

        ChatCompletionAgent agentWriter =
            new()
            {
                Name = CopyWriterName,
                Instructions = """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        Never delimit the response with quotation marks.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """,
                Kernel = builder.Build(),
                LoggerFactory = LoggerFactory,
            };

        KernelFunction selectionFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
                Determine which participant takes the next turn in a conversation based on the the most recent participant.
                State only the name of the participant to take the next turn.
                No participant should take more than one turn in a row.

                Choose only from these participants:
                - {{{ReviewerName}}}
                - {{{CopyWriterName}}}

                Always follow these rules when selecting the next participant:
                - After {{{CopyWriterName}}}, it is {{{ReviewerName}}}'s turn.
                - After {{{ReviewerName}}}, it is {{{CopyWriterName}}}'s turn.

                History:
                {{$history}}
                """,
                safeParameterNames: "history");

        KernelFunction terminationFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy("""
                Determine if the copy has been approved.  If so, respond with a single word: yes
                History:
                {{$history}}
                """, safeParameterNames: "history");


        AgentGroupChat chat = new(agentWriter, agentReviewer)
        {
            LoggerFactory= LoggerFactory,
            ExecutionSettings = new()
            {
                TerminationStrategy =
                    new KernelFunctionTerminationStrategy(terminationFunction, builder.Build())
                    {
                        // Only the art-director may approve.
                        Agents = [agentReviewer],
                        // Customer result parser to determine if the response is "yes"
                        ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                        // The prompt variable name for the history argument.
                        HistoryVariableName = "history",
                        // Limit total number of turns
                        MaximumIterations = 10,
                    },

                SelectionStrategy =
                            new KernelFunctionSelectionStrategy(selectionFunction, builder.Build())
                            {
                                // Always start with the writer agent.
                                InitialAgent = agentWriter,
                                // Returns the entire result value as a string.
                                ResultParser = (result) => result.GetValue<string>() ?? CopyWriterName,
                                // The prompt variable name for the history argument.
                                HistoryVariableName = "history",
                                // Save tokens by not including the entire history in the prompt
                                // Only include the agent names and not the message content
                                // EvaluateNameOnly = true,
                            },
            }
        };

        // Invoke chat and display messages.
        ChatMessageContent input = new(AuthorRole.User, "concept: one use cups made out of gelatin.");
        chat.AddChatMessage(input);
        input.ConsoleOutputAgentChatMessage();

        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
            response.ConsoleOutputAgentChatMessage();
        }

        Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
    }
}