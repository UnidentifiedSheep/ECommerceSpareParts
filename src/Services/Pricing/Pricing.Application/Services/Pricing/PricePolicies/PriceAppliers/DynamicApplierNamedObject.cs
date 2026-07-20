using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Logic;
using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public class DynamicApplierNamedObject : ApplierNamedObjectBase
{
    public override string NameLocalizationKey => "";
    public override string DescriptionLocalizationKey => "";
    public override string SystemName { get; }
    public override int Order { get; }
    
    private readonly JsonNode _dslRule;
    
    public DynamicApplierNamedObject(
        string systemName,
        int order,
        string dslLogic)
    {
        SystemName = systemName;
        Order = order;
        _dslRule = JsonNode.Parse(dslLogic) 
                   ?? throw new InvalidOperationException("DSL logic is not valid JSON");
    }

    public override ValueTask<PriceCalculationState> ApplyAsync(
        PriceCalculationState state, 
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var data = JsonSerializer.SerializeToNode(state)
                   ?? throw new InvalidOperationException("Failed to serialize price calculation state");

        var result = JsonLogic.Apply(_dslRule, data);
        var appliedPrice = result?.Deserialize<decimal>()
                           ?? throw new InvalidOperationException("DSL logic must return a price");
        //TODO Negative staff gonna look stage

        return ValueTask.FromResult(state with
        {
            SalePrice = appliedPrice,
            AppliedRules =
            [
                ..state.AppliedRules,
                new AppliedPriceRule(
                    Name: SystemName,
                    PriceBefore: state.SalePrice,
                    PriceAfter: appliedPrice)
            ]
        });
    }
}
