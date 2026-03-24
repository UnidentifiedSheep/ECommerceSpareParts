using Testcontainers.PostgreSql;
using Xunit;

namespace Test.Common.TestContainers.Pg;

public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresqlContainer = new PostgreSqlBuilder("postgres:latest")
        .Build();

    public string ConnectionString => _postgresqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgresqlContainer.StartAsync();
        Console.WriteLine("PostgreSQL container started");
    }

    public async Task DisposeAsync()
    {
        await _postgresqlContainer.DisposeAsync().AsTask();
    }
}