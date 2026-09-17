namespace Tests.TestContainers.Combined;

public sealed class TestEnvironmentLease : IAsyncDisposable
{
	private readonly Func<TestEnvironmentSlot, ValueTask> _release;

	private bool _disposed;

	public TestEnvironmentSlot Slot { get; }

	internal TestEnvironmentLease(
		TestEnvironmentSlot slot,
		Func<TestEnvironmentSlot, ValueTask> release)
	{
		Slot = slot;
		_release = release;
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		await _release(Slot);
	}
}
