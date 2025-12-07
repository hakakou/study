using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;

public static class ObjectExtensions
{
    public static async Task InvokeMessage(this OpenAIAssistantAgent agent, string threadId, string userMessage)
    {
        var message = new ChatMessageContent(AuthorRole.User, userMessage);
        //var thread = new OpenAIAssistantAgentThread()
        //await agent.InvokeAsync(threadId, message);
        message.PrintChatMessageContent();

        var responses = agent.InvokeAsync(threadId);
        await foreach (ChatMessageContent response in responses)
        {
            response.PrintChatMessageContent();
        }
    }
}