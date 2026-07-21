using Bogus;
using Pricing.Entities.Offers;
using Pricing.Enums;
using Tests.Abstractions;

namespace Pricing.Integration.Tests.DataBuilders.Offers;

public class PriceOfferDataBuilder(Faker faker) : BuilderBase<PriceOffer>(faker)
{
    public int ProductId { get; private set; } = faker.Random.Int(1, 10_000);
    public int CurrencyId { get; private set; } = 1;
    public string StorageName { get; private set; } = "main";
    public DateTime ExpiresAt { get; private set; } = DateTime.UtcNow.AddDays(1);

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

    public PriceOfferDataBuilder WithStorageName(string storageName)
    {
        StorageName = storageName;
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
            StorageName,
            Faker.Random.Decimal(1m, 10_000m),
            PriceOfferSource.Armtek,
            $"offer-{Faker.Random.Guid():N}",
            Faker.Random.Int(1, 100),
            1,
            1,
            0,
            now.AddDays(1),
            now.AddDays(2),
            95,
            now.AddHours(1),
            ExpiresAt);
    }
}
