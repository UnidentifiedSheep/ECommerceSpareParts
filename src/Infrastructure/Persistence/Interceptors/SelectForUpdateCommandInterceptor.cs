using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors;

public partial class SelectForUpdateCommandInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        ManipulateCommand(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        ManipulateCommand(command);
        return new ValueTask<InterceptionResult<object>>(result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        ManipulateCommand(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        ManipulateCommand(command);
        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    private static void ManipulateCommand(DbCommand command)
    {
        if (command.CommandText.Contains("-- ForUpdate", StringComparison.Ordinal))
            command.CommandText = AppendForUpdate(command.CommandText);
    }

    private static string AppendForUpdate(string commandText)
    {
        if (commandText.Contains("FOR UPDATE", StringComparison.OrdinalIgnoreCase))
            return commandText;

        var rootAlias = RootAliasRegex().Match(commandText).Groups["alias"].Value;
        var forUpdate = string.IsNullOrWhiteSpace(rootAlias)
            ? " FOR UPDATE"
            : $" FOR UPDATE OF {rootAlias}";

        return commandText.EndsWith(';')
            ? string.Concat(commandText.AsSpan(0, commandText.Length - 1), forUpdate, ";")
            : commandText + forUpdate;
    }

    [GeneratedRegex(
        "\\bFROM\\s+.+?\\s+AS\\s+(?<alias>\"[^\"]+\"|[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex RootAliasRegex();
}
