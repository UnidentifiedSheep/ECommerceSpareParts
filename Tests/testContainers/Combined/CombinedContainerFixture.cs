using Tests.testContainers.Pg;
using Tests.testContainers.Redis;

using LRedis = Redis;
namespace Tests.testContainers.Combined;

public class CombinedContainerFixture : IAsyncLifetime
{
    public PostgresContainerFixture Postgres { get; } = new();
    public RedisContainerFixture Redis { get; } = new();

    public string PostgresConnectionString => Postgres.ConnectionString;
    public string RedisConnectionString => Redis.ConnectionString;

    public async Task InitializeAsync()
    {
        await Postgres.InitializeAsync();
        await Redis.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await Redis.DisposeAsync();
        await Postgres.DisposeAsync();
    }
}