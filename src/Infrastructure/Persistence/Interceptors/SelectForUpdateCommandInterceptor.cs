using System.Collections.Concurrent;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors;

public sealed partial class SelectForUpdateCommandInterceptor : DbCommandInterceptor
{
    private static readonly ConcurrentDictionary<string, Regex> RegexCache = new();
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
        return ValueTask.FromResult(result);
    }

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
        return ValueTask.FromResult(result);
    }

    private static void ManipulateCommand(DbCommand command)
    {
        var sql = command.CommandText;

        if (sql.Contains("FOR UPDATE", StringComparison.OrdinalIgnoreCase))
            return;

        var table = GetForUpdateTable(sql);

        if (string.IsNullOrWhiteSpace(table))
            return;

        var aliasMatch = FindTableAlias(sql, table);

        if (!aliasMatch.Success)
            return;

        var alias = aliasMatch.Groups["alias"].Value;
        var forUpdate = ShouldSkipLocked(sql)
            ? $"FOR UPDATE OF {alias} SKIP LOCKED"
            : $"FOR UPDATE OF {alias}";

        command.CommandText = InsertForUpdate(sql, aliasMatch.Index, forUpdate);
    }

    private static string? GetForUpdateTable(string sql)
    {
        var match = ForUpdateOfTagRegex().Match(sql);

        return match.Success
            ? match.Groups["table"].Value.Trim()
            : null;
    }

    private static Match FindTableAlias(string sql, string table)
    {
        var regex = RegexCache.GetOrAdd(table, static table =>
        {
            var parts = table.Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

            var tablePattern = parts.Length switch
            {
                1 => QuoteOrRaw(parts[0]),
                2 => $@"{QuoteOrRaw(parts[0])}\s*\.\s*{QuoteOrRaw(parts[1])}",
                _ => throw new InvalidOperationException(
                    $"Invalid table name: {table}")
            };

            return new Regex(
                $@"\bFROM\s+{tablePattern}\s+(?:AS\s+)?(?<alias>""[^""]+""|[A-Za-z_][A-Za-z0-9_]*)",
                RegexOptions.IgnoreCase |
                RegexOptions.Compiled);
        });

        return regex.Match(sql);
    }

    private static string InsertForUpdate(string sql, int tableFromIndex, string forUpdate)
    {
        var limitMatch = LimitRegex().Match(sql, tableFromIndex);

        if (limitMatch.Success)
            return sql.Insert(limitMatch.Index, $"{forUpdate} ");

        var semicolon = sql.LastIndexOf(';');

        if (semicolon >= 0)
            return sql.Insert(semicolon, $"{Environment.NewLine}{forUpdate}");

        return sql.TrimEnd() + $"{Environment.NewLine}{forUpdate}";
    }

    private static string QuoteOrRaw(string name)
    {
        var escaped = Regex.Escape(name);

        return $@"(?:""{escaped}""|{escaped})";
    }

    [GeneratedRegex(@"--\s*ForUpdateOf:(?<table>[^\r\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ForUpdateOfTagRegex();

    private static bool ShouldSkipLocked(string sql)
    {
        return SkipLockedTagRegex().IsMatch(sql);
    }

    [GeneratedRegex(@"--\s*SkipLocked\b", RegexOptions.IgnoreCase)]
    private static partial Regex SkipLockedTagRegex();

    [GeneratedRegex(@"\bLIMIT\b", RegexOptions.IgnoreCase)]
    private static partial Regex LimitRegex();
}
