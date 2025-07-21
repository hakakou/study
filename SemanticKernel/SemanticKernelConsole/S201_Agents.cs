using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class S201_Agents : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var agent = new ChatCompletionAgent()
        {
            Name = "Parrot",
            Instructions = "Repeat the user message in the voice of a pirate",
            Kernel = builder.Build(),
        };

        ChatHistory chat = [];
        ChatMessageContent message = new(AuthorRole.User, "this is my new car");
        chat.Add(message);

        message.ConsoleOutputAgentChatMessage();
        await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
        {
            chat.Add(response);
            response.ConsoleOutputAgentChatMessage();
        }

        // Compare to
        // var chat = kernel.GetRequiredService<IChatCompletionService>();
        // chat.GetChatMessageContentAsync(chatHistory);
    }
}