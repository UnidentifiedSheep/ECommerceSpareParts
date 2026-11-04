using Bogus;
using Pricing.Entities.Offers;
using Tests.Abstractions;

namespace Pricing.Integration.Tests.DataBuilders.Offers;

public class ProductPriceOptionDataBuilder(Faker faker) : BuilderBase<ProductPriceOption>(faker)
{
	public Guid PriceOfferId { get; private set; } = faker.Random.Guid();

	public string MarkupVersion { get; private set; } = "markup-v1";

	public string AppliersVersion { get; private set; } = "appliers-v1";

	public Guid PricingSettingsVersion { get; private set; } = faker.Random.Guid();

	public decimal Price { get; private set; } = 100m;

	public decimal Score { get; private set; } = 1m;

	public int CurrencyId { get; private set; } = 1;

	public string StorageCode { get; private set; } = "main";

	public ProductPriceOptionDataBuilder WithPrice(decimal price)
	{
		Price = price;
		return this;
	}

	public ProductPriceOptionDataBuilder WithScore(decimal score)
	{
		Score = score;
		return this;
	}

	public ProductPriceOptionDataBuilder WithCurrencyId(int currencyId)
	{
		CurrencyId = currencyId;
		return this;
	}

	public ProductPriceOptionDataBuilder WithStorageCode(string storageCode)
	{
		StorageCode = storageCode;
		return this;
	}

	public ProductPriceOptionDataBuilder WithPriceOfferId(Guid priceOfferId)
	{
		PriceOfferId = priceOfferId;
		return this;
	}

	public ProductPriceOptionDataBuilder WithVersions(
		string markupVersion,
		string appliersVersion,
		Guid pricingSettingsVersion)
	{
		MarkupVersion = markupVersion;
		AppliersVersion = appliersVersion;
		PricingSettingsVersion = pricingSettingsVersion;
		return this;
	}

	public override ProductPriceOption Build()
	{
		return ProductPriceOption.Create(
			PriceOfferId,
			MarkupVersion,
			AppliersVersion,
			PricingSettingsVersion,
			StorageCode,
			Score,
			Price,
			CurrencyId,
			0.2m,
			TimeSpan.FromDays(1),
			TimeSpan.FromDays(2),
			95);
	}
}
