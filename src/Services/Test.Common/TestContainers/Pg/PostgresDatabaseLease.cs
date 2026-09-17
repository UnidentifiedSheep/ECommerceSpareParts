namespace Tests.TestContainers.Pg;

public sealed class PostgresDatabaseLease : IAsyncDisposable
{
	private readonly Func<PostgresDatabaseSlot, ValueTask> _release;
	private bool _disposed;

	public PostgresDatabaseSlot Database { get; }

	internal PostgresDatabaseLease(
		PostgresDatabaseSlot database,
		Func<PostgresDatabaseSlot, ValueTask> release)
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
