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
        message.ConsoleOutputAgentChatMessage();

        var responses = agent.InvokeAsync(threadId);
        await foreach (ChatMessageContent response in responses)
        {
            response.ConsoleOutputAgentChatMessage();
        }
    }

    public static void ConsoleOutputAgentChatMessage(this ChatMessageContent message)
    {
        // Include ChatMessageContent.AuthorName in output, if present.
        string authorExpression = message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";

        // Include TextContent (via ChatMessageContent.Content), if present.
        string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;

        bool isCode = message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false;

        string codeMarker = isCode ? "\n  [CODE]\n" : " ";
        Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");

        // Provide visibility for inner content (that isn't TextContent).
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {annotation.Quote}: File #{annotation.FileId}");
            }
            else if (item is FileReferenceContent fileReference)
            {
                Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
            }
            else if (item is ImageContent image)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}");
            }
            else if (item is FunctionCallContent functionCall)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
            }
            else if (item is FunctionResultContent functionResult)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result?.AsJson() ?? "*"}");
            }
        }

        if (message.Metadata?.TryGetValue("Usage", out object? usage) ?? false)
        {
            if (usage is RunStepTokenUsage assistantUsage)
            {
                WriteUsage(assistantUsage.TotalTokenCount, assistantUsage.InputTokenCount, assistantUsage.OutputTokenCount);
            }
            else if (usage is OpenAI.Chat.ChatTokenUsage chatUsage)
            {
                WriteUsage(chatUsage.TotalTokenCount, chatUsage.InputTokenCount, chatUsage.OutputTokenCount);
            }
        }

        void WriteUsage(int totalTokens, int inputTokens, int outputTokens)
        {
            Console.WriteLine($"  [Usage] Tokens: {totalTokens}, Input: {inputTokens}, Output: {outputTokens}");
        }
    }
}