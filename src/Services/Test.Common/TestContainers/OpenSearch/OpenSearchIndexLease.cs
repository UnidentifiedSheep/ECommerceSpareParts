namespace Tests.TestContainers.OpenSearch;

public sealed class OpenSearchIndexLease : IAsyncDisposable
{
	private readonly Func<OpenSearchIndexSlot, ValueTask> _release;
	private bool _disposed;

	internal OpenSearchIndexLease(OpenSearchIndexSlot indices, Func<OpenSearchIndexSlot, ValueTask> release)
	{
		Indices = indices;
		_release = release;
	}

	public OpenSearchIndexSlot Indices { get; }

	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;
		_disposed = true;
		await _release(Indices);
	}
}
