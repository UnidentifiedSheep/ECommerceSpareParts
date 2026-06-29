using Application.Common.Interfaces.Settings;
using Enums;
using Main.Entities.Setting;
using Main.Persistence.Context;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders.Currency;

namespace Tests.TestContexts.Currency;

public class CurrencyTestContext(
    DContext context,
    ISettingsService settingsService
) : TestContextBase<DContext>(context)
{
    private readonly List<Main.Entities.Currency.Currency> _currencies = [];
    public IReadOnlyList<Main.Entities.Currency.Currency> Currencies => _currencies;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var created = await new CurrencyBuilder(Faker)
            .BuildManyAndAddToDb(DbContext, 3);

        await settingsService.SetSetting(new CurrencySetting(new CurrencySettingData
        {
            BaseCurrencyId = created.First().Id,
            RateProvider = ExchangeRateProvider.Cbr
        }), cancellationToken);

        _currencies.AddRange(created);
    }
}