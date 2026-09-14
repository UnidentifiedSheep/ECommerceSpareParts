using Locan.Core.Interfaces;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public sealed class MarkupApplier(IMarkupCalculator calculator)
	: ApplierNamedObjectBase, IInternalPriceApplier, ISupplierPriceApplier
{
	public override string SystemName => nameof(MarkupApplier);

	public override ILocalizableMessage NameLocalizationMessage => PriceApplierMarkupNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		PriceApplierMarkupDescriptionMessage.Instance;

	public override int Order => 0;

	public override ValueTask<PriceCalculationState> ApplyAsync(
		PriceCalculationState state,
		CancellationToken ct = default)
	{
		var markupResult = calculator.GetMarkup(state.SalePrice, state.CurrencyId);

		var newState = state with
		{
			BaseMarkup = markupResult,
			SalePrice = markupResult.ResultingPrice,
			AppliedRules =
			[
				.. state.AppliedRules,
				new AppliedPriceRule(
					SystemName,
					state.SalePrice,
					markupResult.ResultingPrice)
			]
		};

		return ValueTask.FromResult(newState);
	}
}
