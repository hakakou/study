using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class DOC_S02_Chat_History(
    IChatCompletionService chatCompletionService, Kernel kernel) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
    }

    public async Task Run()
    {
        // Example 1: Basic chat history
        Utils.PrintSectionHeader("Example 1: Basic Chat History");

        ChatHistory chatHistory = [];
        chatHistory.AddSystemMessage("You are a helpful assistant.");
        chatHistory.AddUserMessage("What's available to order?");
        chatHistory.AddAssistantMessage("We have pizza, pasta, and salad available to order. What would you like to order?");
        chatHistory.AddUserMessage("I'd like to have the first option, please.");

        chatHistory.PrintChatHistory();

        // Example 2: Rich messages with names and images
        Utils.PrintSectionHeader("Example 2: Rich Chat History with Names and Images");
        ChatHistory richHistory = [];

        richHistory.Add(
            new()
            {
                Role = AuthorRole.System,
                Content = "You are a helpful assistant"
            }
        );

        richHistory.Add(
            new()
            {
                Role = AuthorRole.User,
                AuthorName = "Laimonis Dumins",
                Items = [
                    new TextContent { Text = "What available on this menu" },
                    new ImageContent { Uri = new Uri("https://placeholder.pagebee.io/api/random/400/300") }
                ]
            }
        );

        richHistory.Add(
            new()
            {
                Role = AuthorRole.Assistant,
                AuthorName = "Restaurant Assistant",
                Content = "We have pizza, pasta, and salad available to order. What would you like to order?"
            }
        );

        richHistory.Add(
            new()
            {
                Role = AuthorRole.User,
                AuthorName = "Ema Vargova",
                Content = "I'd like to have the first option, please."
            }
        );

        richHistory.PrintChatHistory();

        // Example 3: Simulating function calls for user context
        Utils.PrintSectionHeader("Example 3: Chat History with Simulated Function Calls");
        ChatHistory functionCallHistory = [];

        functionCallHistory.AddSystemMessage("You are a restaurant assistant helping users order food.");
        functionCallHistory.AddUserMessage("I'd like to order pizza.");

        // Add a simulated function call from the assistant
        functionCallHistory.Add(
            new()
            {
                Role = AuthorRole.Assistant,
                Items = [
                    new FunctionCallContent(
                        functionName: "get_user_allergies",
                        pluginName: "User",
                        id: "0001",
                        arguments: new () { {"username", "laimonisdumins"} }
                    ),
                ]
            }
        );

        // Add simulated function results from the tool role
        functionCallHistory.Add(
            new()
            {
                Role = AuthorRole.Tool,
                Items = [
                    new FunctionResultContent(
                        functionName: "get_user_allergies",
                        pluginName: "User",
                        callId: "0001",
                        result: "{ \"allergies\": [\"peanuts\", \"gluten\"] }"
                    )
                ]
            }
        );

        functionCallHistory.PrintChatHistory();

        // Get AI response considering the simulated function results
        functionCallHistory.AddUserMessage("Based on our allergies, what can we safely order?");

        Console.WriteLine("\nGetting AI response with allergy context...");
        var response = await chatCompletionService.GetChatMessageContentAsync(
            functionCallHistory,
            kernel: kernel
        );

        Console.WriteLine($"\nAssistant: {response.Content}");
    }
}