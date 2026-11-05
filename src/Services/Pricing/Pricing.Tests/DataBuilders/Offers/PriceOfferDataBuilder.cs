using Bogus;
using Pricing.Entities.Offers;
using Pricing.Enums;
using Tests.Abstractions;

namespace Pricing.Integration.Tests.DataBuilders.Offers;

public class PriceOfferDataBuilder(Faker faker) : BuilderBase<PriceOffer>(faker)
{
	public int ProductId { get; private set; } = faker.Random.Int(1, 10_000);

	public int CurrencyId { get; private set; } = 1;

	public string StorageCode { get; private set; } = "main";

	public DateTime ExpiresAt { get; private set; } = DateTime.UtcNow.AddDays(1);

	public int AvailableQuantity { get; private set; } = faker.Random.Int(1, 100);

	public PriceOfferSource Source { get; private set; } = PriceOfferSource.Armtek;

	public DateTime OrderTill { get; private set; } = DateTime.UtcNow.AddHours(1);

	public PriceOfferDataBuilder WithAvailableQuantity(int quantity)
	{
		AvailableQuantity = quantity;
		return this;
	}

	public PriceOfferDataBuilder WithSource(PriceOfferSource source)
	{
		Source = source;
		return this;
	}

	public PriceOfferDataBuilder WithOrderTill(DateTime orderTill)
	{
		OrderTill = orderTill;
		return this;
	}

	public PriceOfferDataBuilder WithProductId(int productId)
	{
		ProductId = productId;
		return this;
	}

	public PriceOfferDataBuilder WithCurrencyId(int currencyId)
	{
		CurrencyId = currencyId;
		return this;
	}

	public PriceOfferDataBuilder WithStorageCode(string storageCode)
	{
		StorageCode = storageCode;
		return this;
	}

	public PriceOfferDataBuilder WithExpiresAt(DateTime expiresAt)
	{
		ExpiresAt = expiresAt;
		return this;
	}

	public override PriceOffer Build()
	{
		var now = DateTime.UtcNow;
		return PriceOffer.CreateForSupplier(
			ProductId,
			CurrencyId,
			StorageCode,
			Faker.Random.Decimal(1m, 10_000m),
			Source,
			$"offer-{Faker.Random.Guid():N}",
			AvailableQuantity,
			1,
			1,
			0,
			now.AddDays(1),
			now.AddDays(2),
			95,
			OrderTill,
			ExpiresAt);
	}
}
