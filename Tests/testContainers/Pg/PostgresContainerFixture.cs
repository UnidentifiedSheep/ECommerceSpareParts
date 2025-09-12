using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Testcontainers.PostgreSql;

namespace Tests.testContainers.Pg;

public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public string ConnectionString => _postgresqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgresqlContainer.StartAsync();
        var connectionOptions = new DbContextOptionsBuilder<DContext>().UseNpgsql(ConnectionString).Options;
        await using var context = new DContext(connectionOptions);
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ PostgreSQL container started, database ensured created.");
    }

    public async Task DisposeAsync()
    {
        await _postgresqlContainer.DisposeAsync().AsTask();
    }
}