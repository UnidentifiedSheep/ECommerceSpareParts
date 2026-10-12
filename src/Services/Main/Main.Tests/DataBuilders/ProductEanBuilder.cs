using Bogus;
using Main.Entities.Product;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class ProductEanBuilder(Faker faker) : BuilderBase<ProductEan>(faker)
{
	private static long _nextEan;

	public int? ProductId { get; private set; }

	public string? Ean { get; private set; }

	public ProductEanBuilder WithProductId(int productId)
	{
		ProductId = productId;
		return this;
	}

	public ProductEanBuilder WithEan(string ean)
	{
		Ean = ean;
		return this;
	}

	public override ProductEan Build() => ProductEan.Create(
		ProductId ?? Faker.Random.Int(1),
		Ean ?? Interlocked.Increment(ref _nextEan).ToString("D13"));
}
