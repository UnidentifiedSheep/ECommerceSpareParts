using Application.Common.Interfaces.Settings;
using FluentAssertions;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Currencies;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
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
        cacheRepository
            .Setup(x => x.GetCurrency(baseCurrencyId, cancellationToken))
            .ReturnsAsync(expected);
        var handler = new GetBaseCurrencyHandler(
            settingsService.Object,
            cacheRepository.Object);

        var result = await handler.Handle(
            new GetBaseCurrencyQuery(),
            cancellationToken);

        result.Currency.Should().BeSameAs(expected);
        cacheRepository.Verify(
            x => x.GetCurrency(baseCurrencyId, cancellationToken),
            Times.Once);
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
        var handler = new GetBaseCurrencyHandler(
            settingsService.Object,
            cacheRepository.Object);

        var action = () => handler.Handle(
            new GetBaseCurrencyQuery(),
            CancellationToken.None);

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
    {
        return new CurrencyDto
        {
            Id = id,
            ShortName = "RUB",
            Name = "Russian ruble",
            CurrencySign = "₽",
            Code = "RUB"
        };
    }
}
