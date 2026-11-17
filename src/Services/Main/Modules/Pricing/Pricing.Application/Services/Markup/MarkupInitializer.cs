using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Main.Modules.Currency.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Interfaces.Markup;
using Pricing.Entities;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Markup;

public class MarkupInitializer(
	IMarkupContainer container,
	IReadRepository<MarkupGroup, int> repository,
	ISettingsService settingsService,
	ICurrencyConverter currencyConverter,
	ICurrencyRatesProvider ratesProvider,
	ICurrencyLookup currencyLookup) : IMarkupInitializer
{
	public async Task Initialize(CancellationToken cancellationToken = default)
	{
		var pricingSettings = await settingsService.GetOrDefault<PricingSetting>(cancellationToken);

		var query = pricingSettings.Data.SelectedMarkupId == null
			? repository.Query
				.Where(x => x.SourceMessageId != null)
				.OrderByDescending(x => x.CreatedAt)
				.ThenByDescending(x => x.Id)
			: repository.Query.Where(x => x.Id == pricingSettings.Data.SelectedMarkupId);

		var defaultGroup = await query
			.Include(x => x.MarkupRanges)
			.FirstOrDefaultAsync(cancellationToken);

		if (defaultGroup is null)
		{
			if (pricingSettings.Data.SelectedMarkupId is not null)
				throw new InvalidOperationException(
					"Unable initialize markup service, Markup group not found.");

			var baseCurrencyId = await currencyLookup.GetBaseCurrencyIdAsync(cancellationToken);
			container.Initialize(
				baseCurrencyId,
				new Models.Markup(pricingSettings.Data.DefaultMarkup),
				[],
				[]);
			return;
		}

		var allCurrencyIds = await currencyLookup.GetCurrencyIdsAsync(cancellationToken);

		if (await ratesProvider.GetRateOrDefault(defaultGroup.CurrencyId, cancellationToken) == null)
			throw new InvalidOperationException(
				$"Unable initialize markup service, currency rate not found. CurrencyId: {defaultGroup.CurrencyId}");

		var otherCurrencies = new Dictionary<int, IEnumerable<MarkupRange>>();

		foreach (var currencyId in allCurrencyIds.Where(x => x != defaultGroup.CurrencyId))
		{
			if (await ratesProvider.GetRateOrDefault(currencyId, cancellationToken) == null)
				continue;

			var list = new List<MarkupRange>();
			foreach (var range in defaultGroup.MarkupRanges)
			{
				var start = await currencyConverter.ConvertAsync(
					range.RangeStart,
					defaultGroup.CurrencyId,
					currencyId,
					cancellationToken);

				var end = await currencyConverter.ConvertAsync(
					range.RangeEnd,
					defaultGroup.CurrencyId,
					currencyId,
					cancellationToken);

				list.Add(
					MarkupRange.Create(
						start,
						end,
						range.Markup));
			}

			otherCurrencies[currencyId] = list;
		}

		container.Initialize(
			defaultGroup.CurrencyId,
			new Models.Markup(pricingSettings.Data.DefaultMarkup),
			defaultGroup.MarkupRanges,
			otherCurrencies);
	}
}
