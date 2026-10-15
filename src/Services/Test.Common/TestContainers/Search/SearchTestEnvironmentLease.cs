namespace Tests.TestContainers.Search;

public sealed class SearchTestEnvironmentLease : IAsyncDisposable
{
	private readonly Func<SearchTestEnvironmentSlot, ValueTask> _release;
	private bool _disposed;

	internal SearchTestEnvironmentLease(SearchTestEnvironmentSlot slot, Func<SearchTestEnvironmentSlot, ValueTask> release)
	{
		Slot = slot;
		_release = release;
	}

	public SearchTestEnvironmentSlot Slot { get; }

	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;
		_disposed = true;
		await _release(Slot);
	}
}
