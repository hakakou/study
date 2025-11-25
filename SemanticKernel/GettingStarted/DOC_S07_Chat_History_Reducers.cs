using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System;
using System.Threading.Tasks;


public class DOC_S07_Chat_History_Reducers : ITest
{
    public async Task Run()
    {
        //await Example1_TruncationReducer();
        await Example2_SummarizationReducer();
    }

    private async Task Example1_TruncationReducer()
    {
        Utils.PrintSectionHeader("Example 1: Chat History Truncation Reducer");
        Console.WriteLine("Truncates chat history to a specified size and discards removed messages.");
        Console.WriteLine("System messages are always preserved.\n");

        var chatService = new OpenAIChatCompletionService(
            modelId: "gpt-4o",
            apiKey: Conf.OpenAI.ApiKey);

        var reducer = new ChatHistoryTruncationReducer(targetCount: 2);

        var chatHistory = new ChatHistory("You are a librarian and expert on books about cities");

        string[] userMessages = [
            "Recommend a list of books about Seattle",
            "Recommend a list of books about Dublin",
            "Recommend a list of books about Amsterdam",
            "Recommend a list of books about Paris",
            "Recommend a list of books about London"
        ];

        int totalTokenCount = 0;

        foreach (var userMessage in userMessages)
        {
            chatHistory.AddUserMessage(userMessage);

            Console.WriteLine($">>> User:\n{userMessage}");

            var reducedMessages = await reducer.ReduceAsync(chatHistory);

            Console.WriteLine($"  [Chat history reduced from {chatHistory.Count} to {reducedMessages.Count()} messages]");
            chatHistory = new ChatHistory(reducedMessages);

            var response = await chatService.GetChatMessageContentAsync(chatHistory);

            chatHistory.AddAssistantMessage(response.Content!);

            Console.WriteLine($">>> Assistant:\n{response.Content!}\n");

            if (response.InnerContent is OpenAI.Chat.ChatCompletion chatCompletion)
            {
                var tokenCount = chatCompletion.Usage?.TotalTokenCount ?? 0;
                totalTokenCount += tokenCount;
                Console.WriteLine($"  [Tokens used this turn: {tokenCount}]");
            }
        }

        Console.WriteLine($"\nTotal Token Count: {totalTokenCount}");
    }

    private async Task Example2_SummarizationReducer()
    {
        Utils.PrintSectionHeader("Example 2: Chat History Summarization Reducer");
        Console.WriteLine("Truncates chat history, summarizes removed messages and adds summary back.");
        Console.WriteLine("System messages are always preserved.\n");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var reducer = new ChatHistorySummarizationReducer(
            service: chatService,
            targetCount: 3);

        var chatHistory = new ChatHistory(
            systemMessage: "You are a travel advisor helping plan international trips");

        string[] userMessages = [
            "I'm planning a trip to Japan. What should I know?",
            "What's the best time to visit?",
            "How many days should I spend in Tokyo?",
            "What about Kyoto?",
            "Can you recommend some must-see temples?"
        ];

        int totalTokenCount = 0;

        foreach (var userMessage in userMessages)
        {
            chatHistory.AddUserMessage(userMessage);

            Console.WriteLine($">>> User:\n{userMessage}");

            var reducedMessages = await reducer.ReduceAsync(chatHistory);

            Console.WriteLine($"  [Chat history reduced and summarized from {chatHistory.Count} to {reducedMessages.Count()} messages]");
            chatHistory = new ChatHistory(reducedMessages);

            var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);

            chatHistory.AddAssistantMessage(response.Content!);

            Console.WriteLine($">>> Assistant:\n{response.Content!}\n");

            if (response.InnerContent is OpenAI.Chat.ChatCompletion chatCompletion)
            {
                var tokenCount = chatCompletion.Usage?.TotalTokenCount ?? 0;
                totalTokenCount += tokenCount;
                Console.WriteLine($"  [Tokens used this turn: {tokenCount}]");
            }
        }

        Console.WriteLine($"\nTotal Token Count: {totalTokenCount}");
    }
}
