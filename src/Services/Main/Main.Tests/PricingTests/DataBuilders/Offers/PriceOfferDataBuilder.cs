using Bogus;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Tests.Abstractions;

namespace Tests.PricingTests.DataBuilders.Offers;

public class PriceOfferDataBuilder(Faker faker) : BuilderBase<PriceOffer>(faker)
{
	public int ProductId { get; private set; } = faker.Random.Int(1, 10_000);

	public int CurrencyId { get; private set; } = 1;

	public string StorageCode { get; private set; } = "main";

	public DateTime ExpiresAt { get; private set; } = DateTime.UtcNow.AddDays(1);

	public int AvailableQuantity { get; private set; } = faker.Random.Int(1, 100);

	public PriceOfferSource Source { get; private set; } = PriceOfferSource.Armtek;

	public DateTime OrderTill { get; private set; } = DateTime.UtcNow.AddHours(1);

	public decimal? Price { get; private set; }

	public DateTime? DeliveryDate { get; private set; }

	public PriceOfferDataBuilder WithPrice(decimal price)
	{
		Price = price;
		return this;
	}

	public PriceOfferDataBuilder WithDeliveryDate(DateTime deliveryDate)
	{
		DeliveryDate = deliveryDate;
		return this;
	}

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
		var deliveryDate = DeliveryDate ?? now.AddDays(1);
		return PriceOffer.CreateForSupplier(
			ProductId,
			CurrencyId,
			StorageCode,
			Price ?? Faker.Random.Decimal(1m, 10_000m),
			Source,
			$"offer-{Faker.Random.Guid():N}",
			AvailableQuantity,
			1,
			1,
			0,
			deliveryDate,
			deliveryDate.AddDays(1),
			95,
			OrderTill,
			ExpiresAt);
	}
}
