using Microsoft.Agents.AI;
using Spectre.Console;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public static class Utils
{
    private static readonly JsonSerializerOptions s_jsonOptionsCache = new() { WriteIndented = true };
    private static readonly object s_lock = new object();

    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, s_jsonOptionsCache);
    }

    public static async Task<AgentResponse> WriteRunAsync(this AIAgent agent,
        string input, AgentSession session)
    {
        AnsiConsole.MarkupLine($"[deepskyblue1]USER:[/] {Markup.Escape(input)}");

        var output = await agent.RunAsync(input, session);

        AnsiConsole.MarkupLine($"[yellow]AGENT:[/] {Markup.Escape(output.ToString() ?? string.Empty)}");
        return output;
    }

    public static async Task<string> SpySession(this AIAgent agent, AgentSession session)
    {
        JsonElement sessionElement = await agent.SerializeSessionAsync(session);
        return JsonSerializer.Serialize(sessionElement,
            new JsonSerializerOptions() { WriteIndented = true });
    }
}
