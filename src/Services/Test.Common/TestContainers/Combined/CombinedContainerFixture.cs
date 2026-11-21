using System.Threading.Channels;
using Tests.TestContainers.Pg;
using Tests.TestContainers.Redis;

namespace Tests.TestContainers.Combined;

public sealed class CombinedContainerFixture : IAsyncLifetime
{
	private const int SlotCount = AssemblyFixture.MaxThreads;
	private const int ReadScenarioSlotCount = AssemblyFixture.ReadScenarioSlots;
	private readonly Lazy<Task> _initialization;
	private readonly Lazy<Task<Channel<TestEnvironmentSlot>>> _readScenarioSlots;
	private readonly Lazy<PostgresContainerFixture> _postgres = new(() => new PostgresContainerFixture());

	private readonly List<PostgresDatabaseLease> _postgresLeases = [];
	private readonly Lazy<RedisContainerFixture> _redis = new(() => new RedisContainerFixture());
	private readonly List<RedisDatabaseLease> _redisLeases = [];

	private Channel<TestEnvironmentSlot> _slots = null!;

	public CombinedContainerFixture()
	{
		_initialization = new Lazy<Task>(InitializeCoreAsync);
		_readScenarioSlots = new Lazy<Task<Channel<TestEnvironmentSlot>>>(InitializeReadScenarioSlotsAsync);
	}

	public PostgresContainerFixture Postgres => _postgres.Value;

	public RedisContainerFixture Redis => _redis.Value;

	public ValueTask InitializeAsync() => ValueTask.CompletedTask;

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

	private async Task<Channel<TestEnvironmentSlot>> InitializeReadScenarioSlotsAsync()
	{
		await _initialization.Value;

		var slots = Channel.CreateBounded<TestEnvironmentSlot>(
			new BoundedChannelOptions(ReadScenarioSlotCount)
			{
				SingleReader = false,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.Wait
			});

		for (var i = 0; i < ReadScenarioSlotCount; i++)
		{
			var postgresLease = await Postgres.AcquireDatabaseAsync();
			var redisLease = await Redis.AcquireDatabaseAsync();

			lock (_postgresLeases)
				_postgresLeases.Add(postgresLease);
			lock (_redisLeases)
				_redisLeases.Add(redisLease);

			await slots.Writer.WriteAsync(
				new TestEnvironmentSlot(
					SlotCount + i,
					postgresLease.Database.ConnectionString,
					redisLease.Database.ConnectionString));
		}

		return slots;
	}

	private async Task DisposeCoreAsync()
	{
		_slots?.Writer.TryComplete();
		if (_readScenarioSlots.IsValueCreated && _readScenarioSlots.Value.IsCompletedSuccessfully)
			_readScenarioSlots.Value.Result.Writer.TryComplete();

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

	public async ValueTask<TestEnvironmentLease> AcquireReadScenarioAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var slots = await _readScenarioSlots.Value.WaitAsync(cancellationToken);
		var slot = await slots.Reader.ReadAsync(cancellationToken);

		return new TestEnvironmentLease(slot, released => slots.Writer.WriteAsync(released));
	}

	private ValueTask ReleaseAsync(TestEnvironmentSlot slot) => _slots.Writer.WriteAsync(slot);
}
