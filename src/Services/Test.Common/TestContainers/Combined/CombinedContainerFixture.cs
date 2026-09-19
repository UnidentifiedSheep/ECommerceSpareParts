using System.Threading.Channels;
using Tests.TestContainers.Pg;
using Tests.TestContainers.Redis;

namespace Tests.TestContainers.Combined;

public sealed class CombinedContainerFixture : IAsyncLifetime
{
	private const int SlotCount = AssemblyFixture.MaxThreads;

	private readonly List<PostgresDatabaseLease> _postgresLeases = [];
	private readonly List<RedisDatabaseLease> _redisLeases = [];

	private Channel<TestEnvironmentSlot> _slots = null!;

	public PostgresContainerFixture Postgres { get; } = new();

	public RedisContainerFixture Redis { get; } = new();

	public async ValueTask InitializeAsync()
	{
		await Task.WhenAll(Postgres.InitializeAsync().AsTask(), Redis.InitializeAsync().AsTask());

		_slots = Channel.CreateBounded<TestEnvironmentSlot>(
			new BoundedChannelOptions(SlotCount)
			{
				SingleReader = false,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.Wait
			});

		for (var i = 0; i < SlotCount; i++)
		{
			var postgresLease = await Postgres.AcquireDatabaseAsync();
			var redisLease = await Redis.AcquireDatabaseAsync();

			_postgresLeases.Add(postgresLease);
			_redisLeases.Add(redisLease);

			await _slots.Writer.WriteAsync(
				new TestEnvironmentSlot(
					i,
					postgresLease.Database.ConnectionString,
					redisLease.Database.ConnectionString));
		}
	}

	public async ValueTask DisposeAsync()
	{
		_slots.Writer.TryComplete();

		foreach (var lease in _redisLeases)
			await lease.DisposeAsync();

		foreach (var lease in _postgresLeases)
			await lease.DisposeAsync();

		await Redis.DisposeAsync();
		await Postgres.DisposeAsync();
	}

	public async ValueTask<TestEnvironmentLease> AcquireAsync(CancellationToken cancellationToken = default)
	{
		var slot = await _slots.Reader.ReadAsync(cancellationToken);

		return new TestEnvironmentLease(slot, ReleaseAsync);
	}

	private ValueTask ReleaseAsync(TestEnvironmentSlot slot) => _slots.Writer.WriteAsync(slot);
}
