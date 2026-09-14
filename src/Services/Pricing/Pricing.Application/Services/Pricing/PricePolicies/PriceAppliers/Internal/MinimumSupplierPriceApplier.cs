using Application.Common.Interfaces.Currency;
using Locan.Core.Interfaces;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;

public class MinimumSupplierPriceApplier(ICurrencyConverter currencyConverter)
	: ApplierNamedObjectBase, IInternalPriceApplier
{
	public override string SystemName => nameof(MinimumSupplierPriceApplier);

	public override ILocalizableMessage NameLocalizationMessage =>
		PriceApplierMinimumSupplierPriceNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		PriceApplierMinimumSupplierPriceDescriptionMessage.Instance;

	public override int Order => -1000;

	public override async ValueTask<PriceCalculationState> ApplyAsync(
		PriceCalculationState state,
		CancellationToken ct = default)
	{
		if (!state.Market.HasMarket)
			return state;

		var referenceOffer = state.Market.Items.Count > 0 ? state.Market.Items[0] : null;

		var salePriceInBase = await currencyConverter.ConvertToBaseAsync(
			state.SalePrice,
			state.CurrencyId,
			ct);

		if (referenceOffer is null || salePriceInBase >= referenceOffer.CostInBaseCurrency)
			return state;

		var fromBase = await currencyConverter.ConvertFromBaseAsync(
			referenceOffer.CostInBaseCurrency,
			state.CurrencyId,
			ct);

		var newState = state with
		{
			SalePrice = fromBase
		};

		return newState;
	}
}
