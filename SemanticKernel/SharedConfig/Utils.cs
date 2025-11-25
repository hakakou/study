using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Linq;
using System.Text.Json;

public static class Utils
{
    private static readonly JsonSerializerOptions s_jsonOptionsCache = new() { WriteIndented = true };

    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, s_jsonOptionsCache);
    }

    public static void PrintChatHistory(this ChatHistory history)
    {
        int messageNumber = 1;
        foreach (var message in history)
        {
            var authorName = !string.IsNullOrEmpty(message.AuthorName) ? $" ({message.AuthorName})" : "";
            Console.WriteLine($"\n{messageNumber}) {message.Role}{authorName}:");

            if (message.Items != null && message.Items.Count > 0)
            {
                foreach (var item in message.Items)
                {
                    switch (item)
                    {
                        case TextContent textContent:
                            Console.WriteLine($"  [Text] {textContent.Text}");
                            break;
                        case ImageContent imageContent:
                            Console.WriteLine($"  [Image] {imageContent.Uri}");
                            break;
                        case FunctionCallContent functionCall:
                            Console.WriteLine($"  [FunctionCall] {functionCall.PluginName}.{functionCall.FunctionName}");
                            Console.WriteLine($"    Id: {functionCall.Id}");
                            if (functionCall.Arguments != null)
                            {
                                Console.WriteLine($"    Arguments: {string.Join(", ", functionCall.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                            }
                            break;
                        case FunctionResultContent functionResult:
                            Console.WriteLine($"  [FunctionResult] {functionResult.PluginName}.{functionResult.FunctionName}");
                            Console.WriteLine($"    CallId: {functionResult.CallId}");
                            Console.WriteLine($"    Result: {functionResult.Result}");
                            break;
                        default:
                            Console.WriteLine($"  [Unknown] {item}");
                            break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(message.Content))
            {
                Console.WriteLine($"  {message.Content}");
            }

            if (message.Metadata?.TryGetValue("Usage", out object? usage) ?? false)
            {
                if (usage != null)
                {
                    var usageType = usage.GetType();
                    var totalTokensProp = usageType.GetProperty("TotalTokenCount");
                    var inputTokensProp = usageType.GetProperty("InputTokenCount");
                    var outputTokensProp = usageType.GetProperty("OutputTokenCount");

                    if (totalTokensProp != null && inputTokensProp != null && outputTokensProp != null)
                    {
                        var totalTokens = totalTokensProp.GetValue(usage);
                        var inputTokens = inputTokensProp.GetValue(usage);
                        var outputTokens = outputTokensProp.GetValue(usage);
                        Console.WriteLine($"  [Usage] Tokens: {totalTokens}, Input: {inputTokens}, Output: {outputTokens}");
                    }
                }
            }

            messageNumber++;
        }
    }

    public static void PrintSectionHeader(string title, char separatorChar = '=', int width = 80)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string(separatorChar, width - 1));
    }
}
