using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System;
using System.Threading.Tasks;
using static SharedConfig.Conf;

[RunDirectlyAttribute]
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

        var chatHistory = new ChatHistory(
            systemMessage: "You are a librarian and expert on books about cities. Respond with max 200 chars.");

        string[] userMessages = [
            "Recommend a list of books about Seattle",
            "Recommend a list of books about Dublin",
            "Recommend a list of books about Amsterdam",
            "Recommend a list of books about Paris",
            "Recommend a list of books about London"
        ];

        foreach (var userMessage in userMessages)
        {
            Console.WriteLine($">>> User:\n{userMessage}");
            chatHistory.AddUserMessage(userMessage);

            var reducedMessages = await reducer.ReduceAsync(chatHistory);
            chatHistory = new ChatHistory(reducedMessages);
            chatHistory.PrintChatHistory();

            var response = await chatService.GetChatMessageContentAsync(chatHistory);
            Console.WriteLine($">>> Assistant:\n{response.Content!}\n");
            chatHistory.AddAssistantMessage(response.Content!);
        }
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
            service: chatService, targetCount: 2);

        var chatHistory = new ChatHistory(
            systemMessage: "You are a travel advisor helping plan international trips. Respond with max 200 chars.");

        string[] userMessages = [
            "I'm planning a trip to Japan. What should I know?",
            "What's the best time to visit?",
            "How many days should I spend in Tokyo?",
            "What about Kyoto?",
            "Can you recommend some must-see temples?"
        ];

        foreach (var userMessage in userMessages)
        {
            Console.WriteLine($">>> User:\n{userMessage}");
            chatHistory.AddUserMessage(userMessage);

            var reducedMessages = await reducer.ReduceAsync(chatHistory);
            if (reducedMessages != null)
                chatHistory = new ChatHistory(reducedMessages);
            chatHistory.PrintChatHistory();

            var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
            Console.WriteLine($">>> Assistant:\n{response.Content!}\n");
            chatHistory.AddAssistantMessage(response.Content!);
        }
    }
}
