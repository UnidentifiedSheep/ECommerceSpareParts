using System.Threading.Channels;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace Tests.TestContainers.Redis;

public sealed class RedisContainerFixture : IAsyncLifetime
{
	private const int DatabaseCount = AssemblyFixture.MaxThreads;

	private readonly RedisContainer _redisContainer =
		new RedisBuilder("redis/redis-stack:latest")
			.WithPortBinding(6379, true)
			.Build();

	private Channel<RedisDatabaseSlot> _databasePool = null!;

	public string BaseConnectionString =>
		$"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";

	public async ValueTask InitializeAsync()
	{
		await _redisContainer.StartAsync();

		_databasePool = Channel.CreateBounded<RedisDatabaseSlot>(
			new BoundedChannelOptions(DatabaseCount)
			{
				SingleReader = false,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.Wait
			});

		for (var i = 0; i < DatabaseCount; i++)
		{
			await _databasePool.Writer.WriteAsync(
				new RedisDatabaseSlot(
					i,
					BuildConnectionString(i)));
		}

		Console.WriteLine(
			$"Redis container started with {DatabaseCount} logical databases.");
	}

	public async ValueTask<RedisDatabaseLease> AcquireDatabaseAsync(
		CancellationToken cancellationToken = default)
	{
		var slot = await _databasePool.Reader.ReadAsync(cancellationToken);

		return new RedisDatabaseLease(
			slot,
			ReleaseDatabaseAsync);
	}

	private ValueTask ReleaseDatabaseAsync(RedisDatabaseSlot slot) => _databasePool.Writer.WriteAsync(slot);

	private string BuildConnectionString(int database)
	{
		var options = ConfigurationOptions.Parse(BaseConnectionString);
		options.DefaultDatabase = database;

		return options.ToString();
	}

	public ValueTask DisposeAsync() => _redisContainer.DisposeAsync();
}
