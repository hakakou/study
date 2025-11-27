using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Resources;
using SharedConfig;

public class S205_JsonResult : ITest
{
    private const int ScoreCompletionThreshold = 70;

    private const string TutorName = "Tutor";

    private const string TutorInstructions =
        """
    Think step-by-step and rate the user input on creativity and expressiveness from 1-100.

    Respond in JSON format with the following JSON schema:

    {
        "score": "integer (1-100)",
        "notes": "the reason for your score"
    }
    """;

    public async Task Run()
    {
        // Define the agents
        ChatCompletionAgent agent =
            new()
            {
                Instructions = TutorInstructions,
                Name = TutorName,
                Kernel = Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion(
                        modelId: "gpt-4o",
                        apiKey: Conf.OpenAI.ApiKey)
                    .Build(),
            };

        // Create a chat for agent interaction.
        AgentGroupChat chat =
            new()
            {
                ExecutionSettings =
                    new()
                    {
                        // Here a TerminationStrategy subclass is used that will terminate when
                        // the response includes a score that is greater than or equal to 70.
                        TerminationStrategy = new ThresholdTerminationStrategy()
                    }
            };

        // Respond to user input
        await InvokeAgentAsync("The sunset is very colorful.");
        await InvokeAgentAsync("The sunset is setting over the mountains.");
        await InvokeAgentAsync("The sunset is setting over the mountains and filled the sky with a deep red flame, setting the clouds ablaze.");

        // Local function to invoke agent and display the conversation messages.
        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.AddChatMessage(message);
            message.ConsoleOutputAgentChatMessage();

            await foreach (ChatMessageContent response in chat.InvokeAsync(agent))
            {
                response.ConsoleOutputAgentChatMessage();

                Console.WriteLine($"[IS COMPLETED: {chat.IsComplete}]");
            }
        }
    }

    private record struct WritingScore(int score, string notes);

    private sealed class ThresholdTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent,
            IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        {
            string lastMessageContent = history[history.Count - 1].Content ?? string.Empty;

            WritingScore? result = JsonResultTranslator.Translate<WritingScore>(lastMessageContent);

            return Task.FromResult((result?.score ?? 0) >= ScoreCompletionThreshold);
        }
    }
}