using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UsageDetails = Microsoft.Extensions.AI.UsageDetails;

public static class Utils
{
    private static readonly JsonSerializerOptions s_jsonOptionsCache = new() { WriteIndented = true };

    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, s_jsonOptionsCache);
    }

    public static async Task<AgentThread> InvokeAgentAsync(this Agent agent, 
        AgentThread thread, string input, KernelArguments? arguments = null)
    {
        if (agent is null) throw new ArgumentNullException(nameof(agent));

        ChatMessageContent message = new(AuthorRole.User, input);
        message.PrintChatMessageContent();

        await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(message, thread,
            options: new() { KernelArguments = arguments }))
        {
            response.Message.PrintChatMessageContent();
            thread = response.Thread;
        }

        return thread;
    }

    public static void PrintSectionHeader(string title, char separatorChar = '=', int width = 80)
    {
        var rule = new Rule($"[cyan]{title.EscapeMarkup()}[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
    }

    public static void PrintChatMessageContent(this ChatMessageContent message)
    {
        // Include ChatMessageContent.AuthorName in output, if present.
        string authorExpression = message.Role == AuthorRole.User ? string.Empty : FormatAuthor();

        // Determine role color
        var roleColor = message.Role.ToString().ToLower() switch
        {
            "user" => "green",
            "assistant" => "blue",
            "system" => "yellow",
            "tool" => "purple",
            _ => "white"
        };

        // Include TextContent (via ChatMessageContent.Content), if present.
        string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content.EscapeMarkup();

        bool isCode = message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false;

        // Print header
        if (isCode)
        {
            AnsiConsole.MarkupLine($"\n[bold {roleColor}]# {message.Role}{authorExpression.EscapeMarkup()}:[/] [yellow][[CODE]][/]");
            if (!string.IsNullOrWhiteSpace(contentExpression))
            {
                AnsiConsole.MarkupLine($"  {contentExpression}");
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"\n[bold {roleColor}]# {message.Role}{authorExpression.EscapeMarkup()}:[/] {contentExpression}");
        }

        // Provide visibility for inner content (that isn't TextContent).
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                if (annotation.Kind == AnnotationKind.UrlCitation)
                {
                    AnsiConsole.MarkupLine($"  [cyan][[{item.GetType().Name}]][/] {annotation.Label.EscapeMarkup()}: {annotation.ReferenceId.EscapeMarkup()} - {annotation.Title?.EscapeMarkup()}");
                }
                else
                {
                    AnsiConsole.MarkupLine($"  [cyan][[{item.GetType().Name}]][/] {annotation.Label.EscapeMarkup()}: File #{annotation.ReferenceId}");
                }
            }
            else if (item is ActionContent action)
            {
                AnsiConsole.MarkupLine($"  [cyan][[{item.GetType().Name}]][/] {action.Text.EscapeMarkup()}");
            }
            else if (item is ReasoningContent reasoning)
            {
                AnsiConsole.MarkupLine($"  [cyan][[{item.GetType().Name}]][/] {(string.IsNullOrWhiteSpace(reasoning.Text) ? "Thinking..." : reasoning.Text.EscapeMarkup())}");
            }
            else if (item is FileReferenceContent fileReference)
            {
                AnsiConsole.MarkupLine($"  [cyan][[{item.GetType().Name}]][/] File #{fileReference.FileId}");
            }
            else if (item is ImageContent image)
            {
                var imageInfo = image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes";
                AnsiConsole.MarkupLine($"  [magenta][[{item.GetType().Name}]][/] {imageInfo.EscapeMarkup()}");
            }
            else if (item is FunctionCallContent functionCall)
            {
                AnsiConsole.MarkupLine($"  [yellow][[{item.GetType().Name}]][/] {functionCall.Id.EscapeMarkup()}");
            }
            else if (item is FunctionResultContent functionResult)
            {
                var result = functionResult.Result?.AsJson() ?? "*";
                AnsiConsole.MarkupLine($"  [green][[{item.GetType().Name}]][/] {functionResult.CallId.EscapeMarkup()} - {result.EscapeMarkup()}");
            }
        }

        if (message.Metadata?.TryGetValue("Usage", out object? usage) ?? false)
        {
            if (usage is RunStepTokenUsage assistantUsage)
            {
                WriteUsage(assistantUsage.TotalTokenCount, assistantUsage.InputTokenCount, assistantUsage.OutputTokenCount);
            }
            else if (usage is RunStepCompletionUsage agentUsage)
            {
                WriteUsage(agentUsage.TotalTokens, agentUsage.PromptTokens, agentUsage.CompletionTokens);
            }
            else if (usage is OpenAI.Chat.ChatTokenUsage chatUsage)
            {
                WriteUsage(chatUsage.TotalTokenCount, chatUsage.InputTokenCount, chatUsage.OutputTokenCount);
            }
            else if (usage is UsageDetails usageDetails)
            {
                WriteUsage(usageDetails.TotalTokenCount ?? 0, usageDetails.InputTokenCount ?? 0, usageDetails.OutputTokenCount ?? 0);
            }
        }

        string FormatAuthor() => message.AuthorName is not null ? $" - {message.AuthorName}" : string.Empty;

        void WriteUsage(long totalTokens, long inputTokens, long outputTokens)
        {
            AnsiConsole.MarkupLine($"  [orange1][[Usage]][/] Tokens: [bold]{totalTokens}[/], Input: {inputTokens}, Output: {outputTokens}");
        }
    }

    public static void PrintChatHistory(this ChatHistory history)
    {
        var allContent = new List<string>();
        int messageNumber = 1;

        foreach (var message in history)
        {
            var authorName = !string.IsNullOrEmpty(message.AuthorName) ? $" ({message.AuthorName})" : "";

            var roleColor = message.Role.ToString().ToLower() switch
            {
                "user" => "green",
                "assistant" => "blue",
                "system" => "yellow",
                "tool" => "purple",
                _ => "white"
            };

            allContent.Add($"[bold {roleColor}]{messageNumber}) {message.Role}{authorName}:[/]");

            if (message.Items != null && message.Items.Count > 0)
            {
                foreach (var item in message.Items)
                {
                    switch (item)
                    {
                        case TextContent textContent:
                            allContent.Add($"  [cyan][[Text]][/] {textContent.Text.EscapeMarkup()}");
                            break;

                        case ImageContent imageContent:
                            allContent.Add($"  [magenta][[Image]][/] [link]{imageContent.Uri}[/]");
                            break;

                        case FunctionCallContent functionCall:
                            allContent.Add($"  [yellow][[FunctionCall]][/] [bold]{functionCall.PluginName}.{functionCall.FunctionName}[/]");
                            allContent.Add($"    [dim]Id:[/] {functionCall.Id}");
                            if (functionCall.Arguments != null)
                            {
                                allContent.Add($"    [dim]Arguments:[/] {string.Join(", ", functionCall.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                            }
                            break;

                        case FunctionResultContent functionResult:
                            allContent.Add($"  [green][[FunctionResult]][/] [bold]{functionResult.PluginName}.{functionResult.FunctionName}[/]");
                            allContent.Add($"    [dim]CallId:[/] {functionResult.CallId}");
                            allContent.Add($"    [dim]Result:[/] {functionResult.Result?.ToString()?.EscapeMarkup()}");
                            break;

                        default:
                            allContent.Add($"  [red][[Unknown]][/] {item}");
                            break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(message.Content))
            {
                allContent.Add($"  {message.Content.EscapeMarkup()}");
            }

            if (message.Metadata?.TryGetValue("Usage", out object usage) ?? false)
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
                        allContent.Add($"  [orange1][[Usage]][/] Tokens: [bold]{totalTokens}[/], Input: {inputTokens}, Output: {outputTokens}");
                    }
                }
            }

            allContent.Add("");
            messageNumber++;
        }

        var panel = new Panel(string.Join("\n", allContent))
        {
            Header = new PanelHeader("Chat History", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue)
        };

        AnsiConsole.Write(panel);
    }
}