using Bogus;
using Pricing.Entities.Enums;
using Pricing.Entities.Pricing;
using Tests.Abstractions;

namespace Tests.PricingTests.DataBuilders.PriceAppliers;

public class PriceApplierStateDataBuilder(Faker faker) : BuilderBase<PriceApplierState>(faker)
{
	public string? PriceApplierSystemName { get; private set; }

	public PriceOfferSourceType Usage { get; private set; } = PriceOfferSourceType.Supplier;

	public int Order { get; private set; }

	public bool Enabled { get; private set; } = true;

	public PriceApplierStateDataBuilder WithPriceApplierSystemName(string systemName)
	{
		PriceApplierSystemName = systemName;
		return this;
	}

	public PriceApplierStateDataBuilder WithUsage(PriceOfferSourceType usage)
	{
		Usage = usage;
		return this;
	}

	public PriceApplierStateDataBuilder WithOrder(int order)
	{
		Order = order;
		return this;
	}

	public PriceApplierStateDataBuilder WithEnabled(bool enabled)
	{
		Enabled = enabled;
		return this;
	}

	public override PriceApplierState Build()
		=> PriceApplierState.Create(
			PriceApplierSystemName ?? $"price-applier-{Faker.Random.Guid():N}",
			Usage,
			Order,
			Enabled);
}
