using Application.Common.Interfaces.Settings;
using FluentAssertions;
using Main.Entities.Currency;
using Main.Entities.Currency.Settings;
using Main.Modules.Currency.Application.Dtos;
using Main.Modules.Currency.Application.Handlers.Currencies;
using Main.Modules.Currency.Application.Interfaces;
using Moq;

namespace Tests.HandlersTests.Currencies;

public class GetBaseCurrencyHandlerTests
{
	[Fact]
	public async Task Handle_ReturnsBaseCurrencyFromCache()
	{
		const int baseCurrencyId = 42;
		var cancellationToken = new CancellationTokenSource().Token;
		var expected = CreateCurrency(baseCurrencyId);
		var settingsService = CreateSettingsService(baseCurrencyId);
		var cacheRepository = new Mock<ICurrencyCacheRepository>();
		cacheRepository.Setup(x => x.GetCurrency(baseCurrencyId, cancellationToken)).ReturnsAsync(expected);
		var handler = new GetBaseCurrencyHandler(settingsService.Object, cacheRepository.Object);

		var result = await handler.Handle(new GetBaseCurrencyQuery(), cancellationToken);

		result.Currency.Should().BeSameAs(expected);
		cacheRepository.Verify(x => x.GetCurrency(baseCurrencyId, cancellationToken), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenBaseCurrencyMissing_ThrowsCurrencyNotFoundException()
	{
		const int baseCurrencyId = 42;
		var settingsService = CreateSettingsService(baseCurrencyId);
		var cacheRepository = new Mock<ICurrencyCacheRepository>();
		cacheRepository
			.Setup(x => x.GetCurrency(baseCurrencyId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((CurrencyDto?)null);
		var handler = new GetBaseCurrencyHandler(settingsService.Object, cacheRepository.Object);

		var action = () => handler.Handle(new GetBaseCurrencyQuery(), CancellationToken.None);

		await action.Should().ThrowAsync<CurrencyNotFoundException>();
	}

	private static Mock<ISettingsService> CreateSettingsService(int baseCurrencyId)
	{
		var settingsService = new Mock<ISettingsService>();
		settingsService
			.Setup(x => x.GetOrDefault<CurrencySetting>(It.IsAny<CancellationToken>()))
			.ReturnsAsync(
				new CurrencySetting(
					new CurrencySettingData
					{
						BaseCurrencyId = baseCurrencyId
					}));

		return settingsService;
	}

	private static CurrencyDto CreateCurrency(int id)
		=> new()
		{
			Id = id,
			ShortName = "RUB",
			Name = "Russian ruble",
			CurrencySign = "₽",
			Code = "RUB"
		};
}
