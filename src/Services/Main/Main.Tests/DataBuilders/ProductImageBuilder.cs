using Bogus;
using Main.Entities.Product;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class ProductImageBuilder(Faker faker) : BuilderBase<ProductImage>(faker)
{
	private string _extension = ".webp";

	private int _productId = 1;

	public ProductImageBuilder WithProductId(int productId)
	{
		_productId = productId;
		return this;
	}

	public ProductImageBuilder WithExtension(string extension)
	{
		_extension = extension;
		return this;
	}

	public override ProductImage Build() => ProductImage.Create(_productId, _extension);
}
