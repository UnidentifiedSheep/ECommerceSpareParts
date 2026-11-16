using System.Diagnostics.CodeAnalysis;
using Api.Common.HostedServices;
using Application.Common.Interfaces.Lrt;

namespace Application.Common.LRT;

public sealed class LrtQuotaManager : ILrtQuotaManager, IDisposable
{
	private readonly HashSet<Guid> _holders = [];

	private readonly IJobSignal? _releaseSignal;

	private readonly SemaphoreSlim _semaphore;

	private readonly Lock _sync = new();

	private bool _disposed;

	public LrtQuotaManager(string systemName, int maxQuota, IJobSignal? releaseSignal = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(systemName);
		SystemName = systemName;
		MaxQuota = Math.Max(0, maxQuota);
		_releaseSignal = releaseSignal;
		_semaphore = new SemaphoreSlim(MaxQuota, Math.Max(1, MaxQuota));
	}

	public string SystemName { get; }

	public bool IsQuotaAvailable => AvailableQuota > 0;

	public void Dispose()
	{
		lock (_sync)
		{
			if (_disposed)
				return;

			_disposed = true;
			_holders.Clear();
			_semaphore.Dispose();
		}
	}

	public int MaxQuota { get; }

	public int AvailableQuota
	{
		get
		{
			lock (_sync)
			{
				ThrowIfDisposed();
				return _semaphore.CurrentCount;
			}
		}
	}

	public ILrtQuotaLease UseQuota(Guid holderId)
		=> TryUseQuota(holderId, out var quota)
			? quota
			: throw new InvalidOperationException("No available quota.");

	public async ValueTask<ILrtQuotaLease> UseQuotaAsync(
		Guid holderId,
		CancellationToken cancellationToken = default)
	{
		ValidateHolderBeforeWait(holderId);
		await _semaphore.WaitAsync(cancellationToken);

		lock (_sync)
		{
			try
			{
				ThrowIfDisposed();
				AddHolder(holderId);
				return new LrtQuotaLease(this, holderId);
			}
			catch
			{
				if (!_disposed)
					_semaphore.Release();

				throw;
			}
		}
	}

	public bool TryUseQuota(Guid holderId, [NotNullWhen(true)] out ILrtQuotaLease? quota)
	{
		lock (_sync)
		{
			ThrowIfDisposed();
			ValidateHolder(holderId);

			quota = null;

			if (!_semaphore.Wait(0))
				return false;

			AddHolder(holderId);
			quota = new LrtQuotaLease(this, holderId);
			return true;
		}
	}

	private void ValidateHolderBeforeWait(Guid holderId)
	{
		lock (_sync)
		{
			ThrowIfDisposed();
			ValidateHolder(holderId);
		}
	}

	private void ValidateHolder(Guid holderId)
	{
		if (holderId == Guid.Empty)
			throw new ArgumentException("Holder id cannot be empty.", nameof(holderId));

		if (_holders.Contains(holderId))
			throw new InvalidOperationException($"Quota is already acquired for holder '{holderId}'.");
	}

	private void AddHolder(Guid holderId)
	{
		if (!_holders.Add(holderId))
			throw new InvalidOperationException($"Quota is already acquired for holder '{holderId}'.");
	}

	private void ReleaseQuota(Guid holderId)
	{
		var released = false;
		lock (_sync)
		{
			if (_disposed)
				return;

			if (_holders.Remove(holderId))
			{
				_semaphore.Release();
				released = true;
			}
		}

		if (released)
			_releaseSignal?.Notify();
	}

	private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, nameof(LrtQuotaManager));

	private sealed class LrtQuotaLease(LrtQuotaManager manager, Guid holderId) : ILrtQuotaLease
	{
		private int _disposed;

		public Guid HolderId => holderId;

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) == 1)
				return;

			manager.ReleaseQuota(holderId);
		}
	}
}
