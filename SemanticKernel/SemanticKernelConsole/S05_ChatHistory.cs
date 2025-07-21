using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class S05_ChatHistory : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();

        ChatHistory chatHistory = [];
        chatHistory.Add(new ChatMessageContent()
        {
            Role = AuthorRole.System,
            Content = "You are a helpful assistant",
        });

        // Add user message with an image
        chatHistory.Add(new()
        {
            Role = AuthorRole.User,
            AuthorName = "Laimonis Dumins",
            Items = [
                    new TextContent { Text = "what color is this dog?" },
                    new ImageContent { Uri = new Uri("https://picsum.photos/id/237/200/300") }
                ]
        });

        // Add assistant message
        chatHistory.Add(new()
        {
            Role = AuthorRole.Assistant,
            AuthorName = "Restaurant Assistant",
            Content = "We have pizza, pasta, and salad available to order. What would you like to order?"
        });

        // Add additional message from a different user
        chatHistory.Add(new()
        {
            Role = AuthorRole.User,
            AuthorName = "Ema Vargova",
            Content = "I'd like to have the first option, please."
        });

        // Add a simulated function call from the assistant
        chatHistory.Add(new()
        {
            Role = AuthorRole.Assistant,
            Items = [
                new FunctionCallContent(
                functionName: "get_user_allergies",
                pluginName: "User",
                id: "0001",
                arguments: new () { {"username", "laimonisdumins"} }),

            new FunctionCallContent(
                functionName: "get_user_allergies",
                pluginName: "User",
                id: "0002",
                arguments: new() { { "username", "emavargova" } })
            ]
        });

        // Add a simulated function results from the tool role
        chatHistory.Add(new()
        {
            Role = AuthorRole.Tool,
            Items = [
                new FunctionResultContent(
                    functionName: "get_user_allergies",
                    pluginName: "User",
                    callId: "0001",
                    result: "{ \"allergies\": [\"peanuts\", \"gluten\"] }"
                )]
        });

        chatHistory.Add(new()
        {
            Role = AuthorRole.Tool,
            Items = [
                    new FunctionResultContent(
                        functionName: "get_user_allergies",
                        pluginName: "User",
                        callId: "0002",
                        result: "{ \"allergies\": [\"dairy\", \"soy\"] }"
                )]
        });
    }
}