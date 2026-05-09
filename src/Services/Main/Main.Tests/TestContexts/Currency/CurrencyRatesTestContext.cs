using Application.Common.Interfaces.Settings;
using Main.Abstractions.Models.Settings;
using Main.Entities.Currency;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders;

namespace Tests.TestContexts;

public class CurrencyRatesTestContext(
    DContext context,
    CurrencyTestContext currencyTestContext,
    ISettingsService settingsService
) : TestContextBase<DContext>(context), IDependentTestContext
{
    public CurrencyTestContext CurrencyTestContext => currencyTestContext;

    public IReadOnlyCollection<CurrencyRate> Rates { get; private set; } = null!;

    public static Type[] DependsOn { get; } =
    [
        typeof(CurrencyTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var currencySetting = await settingsService.GetOrDefault<CurrencySetting>(cancellationToken);
        var baseCurrencyId = currencySetting.Data.BaseCurrencyId;
        var builders = CurrencyTestContext
            .Currencies
            .Where(x => x.Id != baseCurrencyId)
            .Select(currency => new CurrencyRateBuilder(Faker)
                .WithToCurrencyId(baseCurrencyId)
                .WithFromCurrencyId(currency.Id));

        Rates = await builders.BuildManyCombinedAndAddToDb(DbContext, 1);
    }
}