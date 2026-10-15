using System.Threading.Channels;
using Tests.TestContainers.OpenSearch;
using Tests.TestContainers.Redis;

namespace Tests.TestContainers.Search;

public sealed class SearchContainerFixture : IAsyncLifetime
{
	private const int SlotCount = AssemblyFixture.MaxThreads;
	private readonly List<OpenSearchIndexLease> _indexLeases = [];
	private readonly List<RedisDatabaseLease> _redisLeases = [];
	private Channel<SearchTestEnvironmentSlot>? _slots;

	public OpenSearchContainerFixture OpenSearch { get; } = new();
	public RedisContainerFixture Redis { get; } = new();

	public async ValueTask InitializeAsync()
	{
		await Task.WhenAll(OpenSearch.InitializeAsync().AsTask(), Redis.InitializeAsync().AsTask());
		_slots = Channel.CreateBounded<SearchTestEnvironmentSlot>(new BoundedChannelOptions(SlotCount)
		{
			SingleReader = false,
			SingleWriter = false,
			FullMode = BoundedChannelFullMode.Wait
		});
		for (var i = 0; i < SlotCount; i++)
		{
			var indices = await OpenSearch.AcquireIndicesAsync();
			_indexLeases.Add(indices);
			var redis = await Redis.AcquireDatabaseAsync();
			_redisLeases.Add(redis);
			await _slots.Writer.WriteAsync(new SearchTestEnvironmentSlot(i, indices.Indices, redis.Database));
		}
	}

	public async ValueTask<SearchTestEnvironmentLease> AcquireAsync(CancellationToken cancellationToken = default)
	{
		var slots = _slots ?? throw new InvalidOperationException("Search fixture has not been initialized.");
		var slot = await slots.Reader.ReadAsync(cancellationToken);
		return new SearchTestEnvironmentLease(slot, ReleaseAsync);
	}

	public async ValueTask DisposeAsync()
	{
		_slots?.Writer.TryComplete();
		foreach (var lease in _indexLeases) await lease.DisposeAsync();
		foreach (var lease in _redisLeases) await lease.DisposeAsync();
		try { await Redis.DisposeAsync(); }
		finally { await OpenSearch.DisposeAsync(); }
	}

	private ValueTask ReleaseAsync(SearchTestEnvironmentSlot slot) => _slots!.Writer.WriteAsync(slot);
}
