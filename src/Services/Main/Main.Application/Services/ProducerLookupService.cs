using Main.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application.Services;

public sealed class ProducerLookupService(IServiceScopeFactory scopeFactory) : IProducerLookupService, IDisposable
{
	private readonly Lock _loadLock = new();
	private readonly CancellationTokenSource _shutdown = new();
	private IProducerLookup? _snapshot;
	private Task<IProducerLookup>? _loadTask;
	private long _version;
	private long _loadVersion;
	private bool _disposed;

	public async Task<IProducerLookup> Load(CancellationToken cancellationToken = default)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Task<IProducerLookup> task;
			long version;
			lock (_loadLock)
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				if (_snapshot is not null) return _snapshot;

				if (_loadTask is null || _loadTask.IsFaulted || _loadTask.IsCanceled)
				{
					_loadVersion = _version;
					_loadTask = LoadCore(_shutdown.Token);
				}
				task = _loadTask;
				version = _loadVersion;
			}

			IProducerLookup snapshot;
			try
			{
				// Each caller cancels its own wait, never the shared database load.
				snapshot = await task.WaitAsync(cancellationToken);
			}
			catch
			{
				lock (_loadLock)
				{
					if (ReferenceEquals(_loadTask, task) && (task.IsFaulted || task.IsCanceled))
						_loadTask = null;
				}

				throw;
			}

			lock (_loadLock)
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				if (version == _version)
				{
					_snapshot ??= snapshot;
					if (ReferenceEquals(_loadTask, task))
						_loadTask = null;
					return _snapshot;
				}
				// An invalidation during loading makes this result obsolete.
				if (ReferenceEquals(_loadTask, task))
					_loadTask = null;
			}
		}
	}

	public void Invalidate()
	{
		lock (_loadLock)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			_version++;
			_snapshot = null;
			if (_loadTask?.IsCompleted == true)
				_loadTask = null;
		}
	}

	private async Task<IProducerLookup> LoadCore(CancellationToken cancellationToken)
	{
		await using var scope = scopeFactory.CreateAsyncScope();
		return await scope.ServiceProvider.GetRequiredService<IProducerLookupLoader>().Load(cancellationToken);
	}

	public void Dispose()
	{
		lock (_loadLock)
		{
			if (_disposed) return;
			_disposed = true;
			_snapshot = null;
			_loadTask = null;
		}
		_shutdown.Cancel();
		_shutdown.Dispose();
	}
}
