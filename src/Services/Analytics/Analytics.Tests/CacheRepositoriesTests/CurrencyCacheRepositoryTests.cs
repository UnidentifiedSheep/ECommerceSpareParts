using System.Net;
using Analytics.Application.Interfaces.Cache;
using Analytics.Cache;
using Cache;
using FluentAssertions;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Test.Common.TestContainers.Combined;
using ZiggyCreatures.Caching.Fusion;

namespace Analytics.Integration.Tests.CacheRepositoriesTests;

public class CurrencyCacheRepositoryTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    private IFusionCache _cache = null!;
    private Mock<IMainClient> _mock = null!;
    private Mock<ICurrencyNode> _currencyMock = null!;
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _cache = Sp.GetRequiredService<IFusionCache>();
        _mock = new Mock<IMainClient>();
        _currencyMock = new Mock<ICurrencyNode>();
        
        _mock.SetupGet(x => x.CurrencyNode).Returns(_currencyMock.Object);
    }

    [Fact]
    public async Task GetCurrencyRate_WhenRateNotCached_ReturnsRateFromMainAndCaches()
    {
        const int currencyId = 1;
        var repository = Repository();
        SetupCurrencyRate(currencyId, 2.5m);

        var result = await repository.GetCurrencyRate(currencyId);

        SetupCurrencyRate(currencyId, 3.5m);
        var cachedResult = await repository.GetCurrencyRate(currencyId);

        result.Should().Be(2.5m);
        cachedResult.Should().Be(2.5m);
        VerifyCurrencyRateRequested(currencyId, Times.Once());
    }

    [Fact]
    public async Task GetCurrencyRate_WhenCurrencyNodeReturnsNotFound_ReturnsNullAndDoesNotCache()
    {
        const int currencyId = 2;
        var repository = Repository();
        SetupCurrencyRateNotFound(currencyId);

        var result = await repository.GetCurrencyRate(currencyId);

        SetupCurrencyRate(currencyId, 4.5m);
        var retryResult = await repository.GetCurrencyRate(currencyId);

        result.Should().BeNull();
        retryResult.Should().Be(4.5m);
        VerifyCurrencyRateRequested(currencyId, Times.Exactly(2));
    }

    [Fact]
    public async Task InvalidateCurrencyRate_WhenRateCached_RemovesCachedRate()
    {
        const int currencyId = 3;
        var repository = Repository();
        SetupCurrencyRate(currencyId, 5.5m);

        var cached = await repository.GetCurrencyRate(currencyId);

        SetupCurrencyRate(currencyId, 6.5m);
        await repository.InvalidateCurrencyRate(currencyId);
        var result = await repository.GetCurrencyRate(currencyId);

        cached.Should().Be(5.5m);
        result.Should().Be(6.5m);
        VerifyCurrencyRateRequested(currencyId, Times.Exactly(2));
    }
    
    private ICurrencyCacheRepository Repository() => new CurrencyCacheRepository(_cache, _mock.Object);

    private void SetupCurrencyRate(int currencyId, decimal rate)
    {
        _currencyMock
            .Setup(x => x.GetCurrencyRate(currencyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InternalResponse<decimal>.Ok(rate));
    }

    private void SetupCurrencyRateNotFound(int currencyId)
    {
        SetupCurrencyRateFailure(currencyId, HttpStatusCode.NotFound);
    }

    private void SetupCurrencyRateFailure(
        int currencyId,
        HttpStatusCode statusCode,
        string? error = null)
    {
        _currencyMock
            .Setup(x => x.GetCurrencyRate(currencyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InternalResponse<decimal>.Fail(statusCode, error));
    }

    private void VerifyCurrencyRateRequested(int currencyId, Times times)
    {
        _currencyMock.Verify(
            x => x.GetCurrencyRate(currencyId, It.IsAny<CancellationToken>()),
            times);
    }
}
