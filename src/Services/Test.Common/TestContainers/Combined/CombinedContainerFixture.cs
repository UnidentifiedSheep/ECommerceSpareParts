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
	private readonly Lazy<Task> _initialization;
	private readonly Lazy<PostgresContainerFixture> _postgres = new(() => new());
	private readonly Lazy<RedisContainerFixture> _redis = new(() => new());

	public CombinedContainerFixture()
	{
		_initialization = new Lazy<Task>(InitializeCoreAsync);
	}

	public PostgresContainerFixture Postgres => _postgres.Value;

	public RedisContainerFixture Redis => _redis.Value;

	public ValueTask InitializeAsync() => ValueTask.CompletedTask;

	private async Task InitializeCoreAsync()
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
		if (!_initialization.IsValueCreated) return;

		try
		{
			await _initialization.Value;
		}
		finally
		{
			await DisposeCoreAsync();
		}
	}

	private async Task DisposeCoreAsync()
	{
		_slots?.Writer.TryComplete();

		foreach (var lease in _redisLeases)
			await lease.DisposeAsync();

		foreach (var lease in _postgresLeases)
			await lease.DisposeAsync();

		await Redis.DisposeAsync();
		await Postgres.DisposeAsync();
	}

	public async ValueTask<TestEnvironmentLease> AcquireAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await _initialization.Value.WaitAsync(cancellationToken);
		var slot = await _slots.Reader.ReadAsync(cancellationToken);

		return new TestEnvironmentLease(slot, ReleaseAsync);
	}

	private ValueTask ReleaseAsync(TestEnvironmentSlot slot) => _slots.Writer.WriteAsync(slot);
}
