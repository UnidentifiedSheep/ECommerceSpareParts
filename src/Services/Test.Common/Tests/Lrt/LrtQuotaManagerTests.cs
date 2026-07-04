using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Application.Common.Models;
using Application.Common.Models.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Tests.Lrt;

[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
public class LrtQuotaManagerTests
{
    [Fact]
    public void MaxQuota_PositiveOption_ReturnsConfiguredValue()
    {
        var manager = CreateManager(5);

        manager.MaxQuota.Should().Be(5);
        manager.AvailableQuota.Should().Be(5);
        manager.IsQuotaAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxQuota_LessOrEqualZero_ReturnsZero(int maxParallelPerWorker)
    {
        var manager = CreateManager(maxParallelPerWorker);

        manager.MaxQuota.Should().Be(0);
        manager.AvailableQuota.Should().Be(0);
        manager.IsQuotaAvailable.Should().BeFalse();
    }

    [Fact]
    public void UseQuota_WhenQuotaAvailable_AcquiresQuota()
    {
        var holderId = Guid.NewGuid();
        var manager = CreateManager(2);

        var quota = manager.UseQuota(holderId);

        quota.HolderId.Should().Be(holderId);
        manager.AvailableQuota.Should().Be(1);
        manager.IsQuotaAvailable.Should().BeTrue();
    }

    [Fact]
    public void UseQuota_WhenNoQuotaAvailable_Throws()
    {
        var manager = CreateManager(1);
        manager.UseQuota(Guid.NewGuid());

        var act = () => manager.UseQuota(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
        manager.AvailableQuota.Should().Be(0);
        manager.IsQuotaAvailable.Should().BeFalse();
    }

    [Fact]
    public void UseQuota_EmptyHolderId_Throws()
    {
        var manager = CreateManager(1);

        var act = () => manager.UseQuota(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UseQuota_DuplicateHolderId_Throws()
    {
        var holderId = Guid.NewGuid();
        var manager = CreateManager(2);
        manager.UseQuota(holderId);

        var act = () => manager.UseQuota(holderId);

        act.Should().Throw<InvalidOperationException>();
        manager.AvailableQuota.Should().Be(1);
    }

    [Fact]
    public void TryUseQuota_WhenQuotaAvailable_ReturnsTrueAndAcquiresQuota()
    {
        var holderId = Guid.NewGuid();
        var manager = CreateManager(1);

        var result = manager.TryUseQuota(holderId, out var quota);

        result.Should().BeTrue();
        quota.Should().NotBeNull();
        quota.HolderId.Should().Be(holderId);
        manager.AvailableQuota.Should().Be(0);
        manager.IsQuotaAvailable.Should().BeFalse();
    }

    [Fact]
    public void TryUseQuota_WhenNoQuotaAvailable_ReturnsFalseAndNullQuota()
    {
        var manager = CreateManager(1);
        manager.UseQuota(Guid.NewGuid());

        var result = manager.TryUseQuota(Guid.NewGuid(), out var quota);

        result.Should().BeFalse();
        quota.Should().BeNull();
        manager.AvailableQuota.Should().Be(0);
    }

    [Fact]
    public void TryUseQuota_EmptyHolderId_Throws()
    {
        var manager = CreateManager(1);

        var act = () => manager.TryUseQuota(Guid.Empty, out _);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryUseQuota_DuplicateHolderId_Throws()
    {
        var holderId = Guid.NewGuid();
        var manager = CreateManager(2);
        manager.TryUseQuota(holderId, out _);

        var act = () => manager.TryUseQuota(holderId, out _);

        act.Should().Throw<InvalidOperationException>();
        manager.AvailableQuota.Should().Be(1);
    }

    [Fact]
    public void QuotaDispose_ReleasesQuota()
    {
        var manager = CreateManager(1);
        var quota = manager.UseQuota(Guid.NewGuid());

        quota.Dispose();

        manager.AvailableQuota.Should().Be(1);
        manager.IsQuotaAvailable.Should().BeTrue();
    }

    [Fact]
    public void QuotaDispose_CalledTwice_ReleasesQuotaOnce()
    {
        var manager = CreateManager(2);
        var quota = manager.UseQuota(Guid.NewGuid());

        quota.Dispose();
        quota.Dispose();

        manager.AvailableQuota.Should().Be(2);
    }

    [Fact]
    public void ReleasedHolder_CanAcquireQuotaAgain()
    {
        var holderId = Guid.NewGuid();
        var manager = CreateManager(1);
        using (manager.UseQuota(holderId))
        {
        }

        using var quota = manager.UseQuota(holderId);

        quota.HolderId.Should().Be(holderId);
        manager.AvailableQuota.Should().Be(0);
    }

    [Fact]
    public void AvailableQuota_WhenOptionsDecreaseBelowUsedQuota_ReturnsZero()
    {
        var maxParallelPerWorker = 2;
        var manager = CreateManager(() => maxParallelPerWorker);
        manager.UseQuota(Guid.NewGuid());
        manager.UseQuota(Guid.NewGuid());

        maxParallelPerWorker = 1;

        manager.MaxQuota.Should().Be(1);
        manager.AvailableQuota.Should().Be(0);
        manager.IsQuotaAvailable.Should().BeFalse();
    }

    [Fact]
    public void AvailableQuota_WhenOptionsIncrease_ReturnsAdditionalQuota()
    {
        var maxParallelPerWorker = 1;
        var manager = CreateManager(() => maxParallelPerWorker);
        manager.UseQuota(Guid.NewGuid());

        maxParallelPerWorker = 3;

        manager.MaxQuota.Should().Be(3);
        manager.AvailableQuota.Should().Be(2);
        manager.IsQuotaAvailable.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ClearsQuotasAndBlocksFutureAccess()
    {
        var manager = CreateManager(1);
        var quota = manager.UseQuota(Guid.NewGuid());

        manager.Dispose();

        quota.Dispose();
        var availableQuota = () => manager.AvailableQuota;
        var useQuota = () => manager.UseQuota(Guid.NewGuid());
        var tryUseQuota = () => manager.TryUseQuota(Guid.NewGuid(), out _);
        availableQuota.Should().Throw<ObjectDisposedException>();
        useQuota.Should().Throw<ObjectDisposedException>();
        tryUseQuota.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var manager = CreateManager(1);

        manager.Dispose();
        var act = () => manager.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task TryUseQuota_ConcurrentCalls_AcquiresNoMoreThanMaxQuota()
    {
        const int maxQuota = 4;
        const int callsCount = 64;
        var manager = CreateManager(maxQuota);
        var holderIds = Enumerable
            .Range(0, callsCount)
            .Select(_ => Guid.NewGuid())
            .ToArray();

        var results = await Task.WhenAll(holderIds.Select(holderId => Task.Run(() =>
        {
            var acquired = manager.TryUseQuota(holderId, out var quota);
            return new
            {
                Acquired = acquired,
                Quota = quota
            };
        })));

        results.Count(x => x.Acquired).Should().Be(maxQuota);
        results.Count(x => !x.Acquired).Should().Be(callsCount - maxQuota);
        results.Where(x => x.Acquired).Should().OnlyContain(x => x.Quota != null);
        results.Where(x => !x.Acquired).Should().OnlyContain(x => x.Quota == null);
        manager.AvailableQuota.Should().Be(0);

        foreach (var quota in results.Select(x => x.Quota).OfType<ILrtQuota>())
            quota.Dispose();
    }

    [Fact]
    public async Task UseQuota_ConcurrentCalls_AcquiresNoMoreThanMaxQuota()
    {
        const int maxQuota = 4;
        const int callsCount = 64;
        var manager = CreateManager(maxQuota);
        var holderIds = Enumerable
            .Range(0, callsCount)
            .Select(_ => Guid.NewGuid())
            .ToArray();

        var results = await Task.WhenAll(holderIds.Select(holderId => Task.Run(() =>
        {
            try
            {
                return new
                {
                    Quota = (ILrtQuota?)manager.UseQuota(holderId),
                    Exception = (Exception?)null
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Quota = (ILrtQuota?)null,
                    Exception = (Exception?)ex
                };
            }
        })));

        results.Count(x => x.Quota is not null).Should().Be(maxQuota);
        results.Count(x => x.Exception is InvalidOperationException).Should().Be(callsCount - maxQuota);
        manager.AvailableQuota.Should().Be(0);

        foreach (var quota in results.Select(x => x.Quota).OfType<ILrtQuota>())
            quota.Dispose();
    }

    [Fact]
    public async Task TryUseQuota_ConcurrentDuplicateHolder_AllowsOnlyOneAcquire()
    {
        const int callsCount = 32;
        var holderId = Guid.NewGuid();
        var manager = CreateManager(callsCount);

        var results = await Task.WhenAll(Enumerable.Range(0, callsCount).Select(_ => Task.Run(() =>
        {
            try
            {
                var acquired = manager.TryUseQuota(holderId, out var quota);
                return new
                {
                    Acquired = acquired,
                    Quota = quota,
                    Exception = (Exception?)null
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Acquired = false,
                    Quota = (ILrtQuota?)null,
                    Exception = (Exception?)ex
                };
            }
        })));

        results.Count(x => x.Acquired).Should().Be(1);
        results.Count(x => x.Exception is InvalidOperationException).Should().Be(callsCount - 1);
        manager.AvailableQuota.Should().Be(callsCount - 1);

        results.Single(x => x.Quota is not null).Quota!.Dispose();
    }

    [Fact]
    public async Task QuotaDispose_ConcurrentCalls_ReleasesQuotaOnce()
    {
        var manager = CreateManager(1);
        var quota = manager.UseQuota(Guid.NewGuid());

        await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(quota.Dispose)));

        manager.AvailableQuota.Should().Be(1);
    }

    private static LrtQuotaManager CreateManager(int maxParallelPerWorker)
    {
        return CreateManager(() => maxParallelPerWorker);
    }

    private static LrtQuotaManager CreateManager(Func<int> maxParallelPerWorker)
    {
        var options = new Mock<IOptionsMonitor<LrtExecutorOptions>>();
        options
            .SetupGet(x => x.CurrentValue)
            .Returns(() => new LrtExecutorOptions
            {
                MaxParallelPerWorker = maxParallelPerWorker()
            });

        return new LrtQuotaManager(options.Object);
    }
}
