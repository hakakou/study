using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;


#pragma warning disable SKEXP0110

public class S201_Agents : ITest
{
    public async Task Run()
    {
        var agent = new ChatCompletionAgent()
        {
            Name = "Parrot",
            Instructions = "Repeat the user message in the voice of a pirate",
            Kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    modelId: "gpt-4o",
                    apiKey: Conf.OpenAI.ApiKey)
                .Build(),
        };

        ChatHistory chat = [];
        ChatMessageContent message = new(AuthorRole.User, "this is my new car");
        chat.Add(message);

        message.PrintChatMessageContent();
        await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
        {
            chat.Add(response);
            response.PrintChatMessageContent();
        }

        // Compare to
        // var chat = kernel.GetRequiredService<IChatCompletionService>();
        // chat.GetChatMessageContentAsync(chatHistory);
    }
}