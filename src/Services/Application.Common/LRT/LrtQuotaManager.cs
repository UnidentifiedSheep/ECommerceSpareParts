using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces.Lrt;
using Application.Common.Models;
using Application.Common.Models.Options;
using Microsoft.Extensions.Options;

namespace Application.Common.LRT;

public sealed class LrtQuotaManager(
    IOptionsMonitor<LrtExecutorOptions> options
) : ILrtQuotaManager, IDisposable
{
    private readonly Lock _sync = new();
    private readonly HashSet<Guid> _holders = [];

    private bool _disposed;

    public int MaxQuota => Math.Max(0, options.CurrentValue.MaxParallelPerWorker);

    public int AvailableQuota
    {
        get
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return Math.Max(0, MaxQuota - _holders.Count);
            }
        }
    }
    public bool IsQuotaAvailable => AvailableQuota > 0;


    public ILrtQuota UseQuota(Guid holderId)
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            if (holderId == Guid.Empty)
                throw new ArgumentException("Holder id cannot be empty.", nameof(holderId));

            if (_holders.Contains(holderId))
                throw new InvalidOperationException($"Quota is already acquired by holder '{holderId}'.");

            if (_holders.Count >= MaxQuota)
                throw new InvalidOperationException("No available quota.");

            _holders.Add(holderId);

            return new LrtQuota(this, holderId);
        }
    }
    
    public bool TryUseQuota(
        Guid holderId, 
        [NotNullWhen(true)] 
        out ILrtQuota? quota)
    {
        lock (_sync)
        {
            ThrowIfDisposed();

            quota = null;

            if (holderId == Guid.Empty)
                throw new ArgumentException("Holder id cannot be empty.", nameof(holderId));

            if (_holders.Contains(holderId))
                throw new InvalidOperationException(
                    $"Quota is already acquired for holder '{holderId}'.");

            if (_holders.Count >= MaxQuota)
                return false;

            _holders.Add(holderId);
            quota = new LrtQuota(this, holderId);
            return true;
        }
    }

    private void ReleaseQuota(Guid holderId)
    {
        lock (_sync)
        {
            if (_disposed)
                return;

            _holders.Remove(holderId);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(LrtQuotaManager));
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _disposed = true;
            _holders.Clear();
        }
    }

    private sealed class LrtQuota(
        LrtQuotaManager manager,
        Guid holderId
    ) : ILrtQuota
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