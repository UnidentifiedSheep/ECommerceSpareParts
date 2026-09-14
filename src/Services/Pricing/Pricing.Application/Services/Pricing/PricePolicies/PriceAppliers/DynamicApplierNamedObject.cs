using System.Text.Json;
using System.Text.Json.Nodes;
using Application.Common;
using Json.Logic;
using Locan.Core.Interfaces;
using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public class DynamicApplierNamedObject : ApplierNamedObjectBase
{
	private readonly JsonNode _dslRule;

	public DynamicApplierNamedObject(
		string systemName,
		int order,
		string dslLogic)
	{
		SystemName = systemName;
		Order = order;
		_dslRule = JsonNode.Parse(dslLogic) ??
			throw new InvalidOperationException("DSL logic is not valid JSON");
	}

	public override ILocalizableMessage NameLocalizationMessage => EmptyMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage => EmptyMessage.Instance;

	public override string SystemName { get; }

	public override int Order { get; }

	public override ValueTask<PriceCalculationState> ApplyAsync(
		PriceCalculationState state,
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();

		var data = JsonSerializer.SerializeToNode(state) ??
			throw new InvalidOperationException("Failed to serialize price calculation state");
		var result = JsonLogic.Apply(_dslRule, data);
		var appliedPrice = result?.Deserialize<decimal>() ??
			throw new InvalidOperationException("DSL logic must return a price");

		if (appliedPrice < 0)
			throw new InvalidOperationException("DSL logic must return a non-negative price");

		return ValueTask.FromResult(
			state with
			{
				SalePrice = appliedPrice,
				AppliedRules =
				[
					.. state.AppliedRules,
					new AppliedPriceRule(
						SystemName,
						state.SalePrice,
						appliedPrice)
				]
			});
	}
}
