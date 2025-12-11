using Microsoft.SemanticKernel;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AgentPanels
{
    private readonly Dictionary<string, AgentInfo> agentBox = new();
    private readonly object _lockObject = new();
    private TaskCompletionSource<bool> _updateSignal = new();

    public async Task Start()
    {
        var table = new Table().Centered();

        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            bool completed = false;

            while (!completed)
            {
                await _updateSignal.Task;

                lock (_lockObject)
                {
                    _updateSignal = new TaskCompletionSource<bool>();

                    if (agentBox.Count == 0)
                        continue;

                    table.Rows.Clear();

                    while (table.Columns.Count < agentBox.Count)
                    {
                        var name = agentBox.Keys.ToList()[table.Columns.Count];
                        table.AddColumn(name);
                    }

                    var rends = agentBox.Values
                        .Select(v => new Markup(v.Content.ToString().EscapeMarkup())).ToArray();
                    table.AddRow(columns: rends);

                    ctx.Refresh();
                    completed = agentBox.Values.All(v => v.Completed);
                }
            }
        });
    }

    class AgentInfo
    {
        public int Index;
        public StringBuilder Content = new();
        public bool Completed = false;
    }

    public ValueTask StreamingResultCallback(StreamingChatMessageContent content, bool isFinal)
    {
        lock (_lockObject)
        {
            if (!agentBox.ContainsKey(content.AuthorName))
                agentBox[content.AuthorName] = new AgentInfo() { Index = agentBox.Count + 1 };

            if (content.Content != null)
                agentBox[content.AuthorName].Content.Append(content.Content);

            agentBox[content.AuthorName].Completed = isFinal;

            bool completed = agentBox.Values.All(v => v.Completed);
            _updateSignal.TrySetResult(true);
        }

        return ValueTask.CompletedTask;
    }

}
