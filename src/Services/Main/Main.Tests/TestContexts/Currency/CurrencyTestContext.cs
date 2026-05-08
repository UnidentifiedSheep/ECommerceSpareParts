using Application.Common.Interfaces.Settings;
using Enums;
using Main.Abstractions.Models.Settings;
using Main.Entities.Currency;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders;

namespace Tests.TestContexts;

public class CurrencyTestContext(
    DContext context, 
    IMediator mediator,
    ISettingsService settingsService
    ) : TestContextBase<DContext>(context, mediator)
{
    private readonly List<Currency> _currencies = [];
    public IReadOnlyList<Currency> Currencies => _currencies;
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var created = await new CurrencyBuilder(Faker)
            .BuildManyAndAddToDb(DbContext, 3);

        await settingsService.SetSetting(new CurrencySetting(new CurrencySettingData
        {
            AutoUpdateRates = false,
            BaseCurrencyId = created.First().Id,
            RateProvider = ExchangeRateProvider.Cbr
        }), cancellationToken);
        
        _currencies.AddRange(created);
    }
}