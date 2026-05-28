using System.Data;
using System.Data.Common;
using Persistence.Interceptors;

namespace Infrastructure.Tests.Persistence;

public class SelectForUpdateCommandInterceptorTests
{
    private readonly SelectForUpdateCommandInterceptor _interceptor = new();

    [Fact]
    public void ReaderExecuting_WhenCommandHasForUpdateTag_AddsForUpdateOfRootAlias()
    {
        var command = CreateCommand("""
            -- ForUpdate
            SELECT p.id, p.comment
            FROM public.purchase AS p
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenRootAliasIsQuoted_AddsForUpdateOfQuotedRootAlias()
    {
        var command = CreateCommand("""
            -- ForUpdate
            SELECT "p"."id", "p"."comment"
            FROM "purchase" AS "p"
            WHERE "p"."id" = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF \"p\"", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenCommandHasJoins_LocksOnlyRootAlias()
    {
        var command = CreateCommand("""
            -- ForUpdate
            SELECT p.id, p.comment, p0.id
            FROM public.purchase AS p
            LEFT JOIN public.purchase_content AS p0 ON p.id = p0.purchase_id
            LEFT JOIN public.products AS p1 ON p0.product_id = p1.id
            WHERE p.id = @id
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p", command.CommandText);
        Assert.DoesNotContain("FOR UPDATE OF p, p0", command.CommandText);
        Assert.DoesNotContain("FOR UPDATE OF p0", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenCommandEndsWithSemicolon_AddsForUpdateBeforeSemicolon()
    {
        var command = CreateCommand("""
            -- ForUpdate
            SELECT p.id
            FROM public.purchase AS p;
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE OF p;", command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenCommandHasNoForUpdateTag_DoesNotChangeCommandText()
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
    public void ReaderExecuting_WhenCommandAlreadyHasForUpdate_DoesNotAppendAgain()
    {
        const string sql = """
            -- ForUpdate
            SELECT p.id
            FROM public.purchase AS p
            FOR UPDATE OF p
            """;
        var command = CreateCommand(sql);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.Equal(sql, command.CommandText);
    }

    [Fact]
    public void ReaderExecuting_WhenRootAliasCannotBeFound_FallsBackToForUpdate()
    {
        var command = CreateCommand("""
            -- ForUpdate
            SELECT 1
            """);

        _interceptor.ReaderExecuting(command, null!, default);

        Assert.EndsWith("FOR UPDATE", command.CommandText);
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
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
            set => field = value;
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
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
