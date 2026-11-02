using Application.Common.Interfaces.Settings;
using Main.Entities.Currency;
using Main.Entities.Settings;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Currency;
using Tests.Extensions;
using Tests.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Tests.TestContexts.Currency;

public class CurrencyRatesTestContext(
	DContext context,
	CurrencyTestContext currencyTestContext,
	ISettingsService settingsService) : TestContextBase<DContext>(context), IDependentTestContext
{
	public CurrencyTestContext CurrencyTestContext => currencyTestContext;

	public IReadOnlyCollection<CurrencyRate> Rates { get; private set; } = null!;

	public static Type[] DependsOn { get; } = [typeof(CurrencyTestContext)];

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
	public async Task SetHistoryAsync(CurrencyRate rate,
		IReadOnlyCollection<(DateTime Date, decimal Rate)> rows, CancellationToken cancellationToken)
	{
		await DbContext.CurrencyRates
			.Where(row => row.FromCurrencyId == rate.FromCurrencyId && row.ToCurrencyId == rate.ToCurrencyId)
			.ExecuteDeleteAsync(cancellationToken);
		foreach (var item in rows)
		{
			var row = CurrencyRate.Create(rate.FromCurrencyId, rate.ToCurrencyId, item.Rate);
			await DbContext.AddAsync(row, cancellationToken);
			DbContext.Entry(row).Property(value => value.CreatedAt).CurrentValue = item.Date;
		}
		await DbContext.SaveChangesAsync(cancellationToken);
	}
}
