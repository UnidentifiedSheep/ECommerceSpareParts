using System.Data;
using System.Data.Common;
using Persistence.Interceptors;

namespace Infrastructure.Tests.Persistence;

public class SelectForUpdateCommandInterceptorTests
{
    private readonly SelectForUpdateCommandInterceptor _interceptor = new();

    [Fact]
    public void ReaderExecuting_WhenCommandHasForUpdateOfTag_AddsForUpdateOfAlias()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT p.id, p.comment
            FROM public.purchase AS p
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenCommandHasSkipLockedTag_AddsForUpdateOfAliasSkipLocked()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            -- SkipLocked
            SELECT p.id, p.comment
            FROM public.purchase AS p
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p SKIP LOCKED", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenCommandHasJoin_LocksOnlyTaggedTableAlias()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT p.id, p0.id
            FROM public.purchase AS p
            LEFT JOIN public.purchase_content AS p0 ON p.id = p0.purchase_id
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
        Assert.DoesNotContain("FOR UPDATE OF p0", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenCommandHasSubquery_InsertsForUpdateBeforeInnerLimit()
    {
        var command = CreateCommand("""
                                    -- ForUpdateOf:public.purchase
                                    SELECT s.id, p1.id
                                    FROM (
                                        SELECT p.id, p.comment
                                        FROM public.purchase AS p
                                        LEFT JOIN public.purchase_logistics AS p0 ON p.id = p0.purchase_id
                                        WHERE p.id = @key
                                        LIMIT 1
                                    ) AS s
                                    LEFT JOIN public.purchase_content AS p1 ON s.id = p1.purchase_id
                                    ORDER BY s.id
                                    """);

        _interceptor.ReaderExecuting(command, null!, default);

        var normalized = Normalize(command.CommandText);

        Assert.Contains(
            "WHERE p.id = @key FOR UPDATE OF p LIMIT 1",
            normalized);
    }

    [Fact]
    public void ReaderExecuting_WhenCommandIsSingleLineSubquery_InsertsForUpdateBeforeLimit()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT s.id FROM ( SELECT p.id FROM public.purchase AS p WHERE p.id = @key LIMIT 1 ) AS s ORDER BY s.id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.Contains("WHERE p.id = @key FOR UPDATE OF p LIMIT 1", Normalize(command.CommandText));
    }

    [Fact]
    public void ReaderExecuting_WhenCommandHasSkipLockedAndLimit_InsertsForUpdateSkipLockedBeforeLimit()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            -- SkipLocked
            SELECT s.id FROM ( SELECT p.id FROM public.purchase AS p WHERE p.id = @key LIMIT 1 ) AS s ORDER BY s.id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.Contains(
            "WHERE p.id = @key FOR UPDATE OF p SKIP LOCKED LIMIT 1",
            Normalize(command.CommandText));
    }

    [Fact]
    public void ReaderExecuting_WhenCommandEndsWithSemicolon_AddsForUpdateBeforeSemicolon()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT p.id
            FROM public.purchase AS p;
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p;", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenTableHasNoSchema_AddsForUpdateOfAlias()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:purchase
            SELECT p.id
            FROM purchase AS p
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenTableAndSchemaAreQuoted_AddsForUpdateOfAlias()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT p.id
            FROM "public"."purchase" AS p
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenAliasIsQuoted_AddsForUpdateOfQuotedAlias()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT "p"."id"
            FROM "public"."purchase" AS "p"
            WHERE "p"."id" = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF \"p\"", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenAliasWithoutAs_AddsForUpdateOfAlias()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT p.id
            FROM public.purchase p
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenTaggedTableIsNotRoot_DoesNotChangeCommandText()
    {
        const string sql = """
                           -- ForUpdateOf:public.purchase
                           SELECT p.id, s.id
                           FROM public.storage AS s
                           JOIN public.purchase AS p ON p.storage = s.id
                           WHERE p.id = @id
                           """;

        var command = CreateCommand(sql);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.Equal(sql, command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenNoForUpdateOfTag_DoesNotChangeCommandText()
    {
        const string sql = """
            SELECT p.id
            FROM public.purchase AS p
            """;

        var command = CreateCommand(sql);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.Equal(sql, command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenForUpdateAlreadyExists_DoesNotAppendAgain()
    {
        const string sql = """
            -- ForUpdateOf:public.purchase
            SELECT p.id
            FROM public.purchase AS p
            FOR UPDATE OF p
            """;

        var command = CreateCommand(sql);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.Equal(sql, command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenTaggedTableAliasCannotBeFound_DoesNotChangeCommandText()
    {
        const string sql = """
            -- ForUpdateOf:public.purchase
            SELECT 1
            """;

        var command = CreateCommand(sql);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.Equal(sql, command.CommandText);
    }

    [Fact]
    public async Task ReaderExecutingAsync_WhenCommandHasForUpdateOfTag_AddsForUpdateOfAlias()
    {
        var command = CreateCommand("""
            -- ForUpdateOf:public.purchase
            SELECT p.id
            FROM public.purchase AS p
            """);

        await _interceptor.ReaderExecutingAsync(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
    }

    private static string Normalize(string value)
    {
        return string.Join(' ', value.Split(
            [' ', '\r', '\n', '\t'],
            StringSplitOptions.RemoveEmptyEntries));
    }

    private static TestDbCommand CreateCommand(string commandText)
    {
        return new TestDbCommand
        {
            CommandText = commandText
        };
    }

    private sealed class TestDbCommand : DbCommand
    {
        public override required string CommandText
        {
            get;
#pragma warning disable CS8765
            set => field = value;
#pragma warning restore CS8765
        }

        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection? DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; } = null!;
        protected override DbTransaction? DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }

        public override void Cancel()
        {
        }

        public override int ExecuteNonQuery()
        {
            throw new NotSupportedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotSupportedException();
        }

        public override void Prepare()
        {
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotSupportedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotSupportedException();
        }
    }
}
