using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Test.Infrastructure.Tests;

public class SqlCommandInterceptor : DbCommandInterceptor
{
    public List<string> Commands { get; } = new();
    public void Clear() => Commands.Clear();

    private void CaptureCommand(DbCommand command)
    {
        var commandText = command.CommandText;

        // Append parameters if any exist
        if (command.Parameters.Count > 0)
        {
            var parameters = new List<string>();
            foreach (DbParameter param in command.Parameters)
            {
                var value = param.Value == null || param.Value == DBNull.Value
                    ? "NULL"
                    : param.Value.ToString();
                parameters.Add($"{param.ParameterName} = '{value}'");
            }
            commandText += $"-- {string.Join(", ", parameters)}";
        }

        Commands.Add(commandText);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        CaptureCommand(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        CaptureCommand(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        CaptureCommand(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureCommand(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        CaptureCommand(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        CaptureCommand(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }
}