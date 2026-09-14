using Application.Common.Interfaces.Settings;
using Locan.Core.Interfaces;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Models.Pricing;
using Pricing.Entities;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;

public class UniquenessAdditionalMarkupApplier(ISettingsService settingsService)
	: ApplierNamedObjectBase, IInternalPriceApplier
{
	public override string SystemName => nameof(UniquenessAdditionalMarkupApplier);

	public override ILocalizableMessage NameLocalizationMessage =>
		PriceApplierUniquenessAdditionalMarkupNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		PriceApplierUniquenessAdditionalMarkupDescriptionMessage.Instance;

	public override int Order => 10000;

	public override async ValueTask<PriceCalculationState> ApplyAsync(
		PriceCalculationState state,
		CancellationToken ct = default)
	{
		if (state.Market.HasMarket)
			return state;

		var priceSettings = (await settingsService.GetOrDefault<PricingSetting>(ct)).Data;
		var applied = state.SalePrice * (1 + priceSettings.UniqProductAdditionalMarkup);

		return state with
		{
			SalePrice = applied,
			AppliedRules =
			[
				.. state.AppliedRules,
				new AppliedPriceRule(
					SystemName,
					state.SalePrice,
					applied)
			]
		};
	}
}
