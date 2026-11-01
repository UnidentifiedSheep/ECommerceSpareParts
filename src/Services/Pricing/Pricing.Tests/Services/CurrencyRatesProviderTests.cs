using System.Net;
using Application.Common.Services.Currency;
using FluentAssertions;
using Integrations.Common;
using Internal.Integration.Core.Interfaces.Main;
using Moq;
using Pricing.Application.Interfaces.Cache;
using RatesProvider = Pricing.Application.Services.CurrencyRatesProvider;

namespace Pricing.Integration.Tests.Services;

public class CurrencyRatesProviderTests
{
	[Fact]
	public async Task HistoricalConversion_UsesMainRateForRequestedDate()
	{
		var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var cached = new Mock<ICachedCurrencyProvider>(MockBehavior.Strict);
		var node = new Mock<ICurrencyNode>();
		node.Setup(client => client.GetCurrencyRate(2, date, It.IsAny<CancellationToken>())).ReturnsAsync(Response<decimal>.Ok(2m));
		var main = new Mock<IMainClient>();
		main.SetupGet(client => client.CurrencyNode).Returns(node.Object);
		var converter = new CurrencyConverter(new RatesProvider(cached.Object, main.Object));

		var result = await converter.ConvertToBaseAsync(100m, 2, date, TestContext.Current.CancellationToken);

		result.Should().Be(50m);
		node.Verify(client => client.GetCurrencyRate(2, date, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task HistoricalConversion_MainFailure_DoesNotFallBackToCurrentRate()
	{
		var date = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var cached = new Mock<ICachedCurrencyProvider>(MockBehavior.Strict);
		var node = new Mock<ICurrencyNode>();
		node.Setup(client => client.GetCurrencyRate(2, date, It.IsAny<CancellationToken>())).ReturnsAsync(Response<decimal>.Fail(HttpStatusCode.NotFound));
		var main = new Mock<IMainClient>();
		main.SetupGet(client => client.CurrencyNode).Returns(node.Object);
		var provider = new RatesProvider(cached.Object, main.Object);

		await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetRate(2, date, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task CurrentConversion_StillUsesCurrentRateCache()
	{
		var cached = new Mock<ICachedCurrencyProvider>();
		cached.Setup(cache => cache.GetCurrencyRate(2, It.IsAny<CancellationToken>())).ReturnsAsync(4m);
		var main = new Mock<IMainClient>(MockBehavior.Strict);
		var converter = new CurrencyConverter(new RatesProvider(cached.Object, main.Object));

		(await converter.ConvertToBaseAsync(100m, 2, TestContext.Current.CancellationToken)).Should().Be(25m);
	}
}
