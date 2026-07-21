using Bogus;
using Pricing.Entities.Pricing;
using Pricing.Enums;
using Tests.Abstractions;

namespace Pricing.Integration.Tests.DataBuilders.PriceAppliers;

public class PriceApplierDataBuilder(Faker faker) : BuilderBase<PriceApplier>(faker)
{
    private readonly List<(PriceOfferSourceType Usage, int Order, bool Enabled)> _states = [];

    public string? SystemName { get; private set; }
    public string? Name { get; private set; }
    public string DslLogic { get; private set; } = """{"var":"salePrice"}""";
    public bool IsDynamic { get; private set; } = true;

    public PriceApplierDataBuilder WithSystemName(string systemName)
    {
        SystemName = systemName;
        return this;
    }

    public PriceApplierDataBuilder WithDslLogic(string dslLogic)
    {
        DslLogic = dslLogic;
        IsDynamic = true;
        return this;
    }

    public PriceApplierDataBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public PriceApplierDataBuilder AsLocal()
    {
        IsDynamic = false;
        return this;
    }

    public PriceApplierDataBuilder WithState(
        PriceOfferSourceType usage,
        int order,
        bool enabled = true)
    {
        _states.Add((usage, order, enabled));
        return this;
    }

    public override PriceApplier Build()
    {
        var systemName = SystemName ?? $"price-applier-{Faker.Random.Guid():N}";
        var applier = IsDynamic
            ? PriceApplier.Create(
                systemName,
                Name ?? Faker.Commerce.ProductName(),
                DslLogic)
            : PriceApplier.CreateLocal(systemName);

        foreach (var state in _states)
        {
            applier.AddState(new PriceApplierStateDataBuilder(Faker)
                .WithPriceApplierSystemName(systemName)
                .WithUsage(state.Usage)
                .WithOrder(state.Order)
                .WithEnabled(state.Enabled)
                .Build());
        }

        return applier;
    }
}
