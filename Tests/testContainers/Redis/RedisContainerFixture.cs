using Testcontainers.Redis;

namespace Tests.testContainers.Redis;

public class RedisContainerFixture : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .WithPortBinding(6379, true)
        .Build();

    public string ConnectionString => $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";
    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
        Console.WriteLine("✅ Redis container started.");
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.DisposeAsync().AsTask();
    }
}

