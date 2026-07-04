using Bogus;
using Main.Entities.Purchase;
using Tests.Abstractions;

namespace Tests.DataBuilders.Purchase;

public class PurchaseContentBuilder(Faker faker) : BuilderBase<PurchaseContent>(faker)
{
    public int? ProductId { get; private set; }
    public int? Count { get; private set; }
    public decimal? Price { get; private set; }
    public int? StorageContentId { get; private set; }
    public string? Comment { get; private set; }

    public PurchaseContentBuilder WithProductId(int productId)
    {
        ProductId = productId;
        return this;
    }

    public PurchaseContentBuilder WithCount(int count)
    {
        Count = count;
        return this;
    }

    public PurchaseContentBuilder WithPrice(decimal price)
    {
        Price = price;
        return this;
    }

    public PurchaseContentBuilder WithStorageContentId(int storageContentId)
    {
        StorageContentId = storageContentId;
        return this;
    }

    public PurchaseContentBuilder WithComment(string? comment)
    {
        Comment = comment;
        return this;
    }

    public override PurchaseContent Build()
    {
        return PurchaseContent.Create(
            ProductId ?? Faker.Random.Int(1),
            Count ?? Faker.Random.Int(1, 100),
            Price ?? Math.Round(Faker.Random.Decimal(1, 1000), 2),
            StorageContentId ?? Faker.Random.Int(1),
            Comment);
    }
}