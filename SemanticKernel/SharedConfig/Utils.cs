using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class Utils
{
    private static readonly JsonSerializerOptions s_jsonOptionsCache = new() { WriteIndented = true };

    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, s_jsonOptionsCache);
    }

    public static void PrintSectionHeader(string title, char separatorChar = '=', int width = 80)
    {
        var rule = new Rule($"[cyan]{title.EscapeMarkup()}[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);
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