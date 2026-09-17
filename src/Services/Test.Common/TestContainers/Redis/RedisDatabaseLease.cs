namespace Tests.TestContainers.Redis;

public sealed class RedisDatabaseLease : IAsyncDisposable
{
	private readonly Func<RedisDatabaseSlot, ValueTask> _release;
	private bool _disposed;

	public RedisDatabaseSlot Database { get; }

	internal RedisDatabaseLease(
		RedisDatabaseSlot database,
		Func<RedisDatabaseSlot, ValueTask> release)
	{
		Database = database;
		_release = release;
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await _release(Database);
	}
}
