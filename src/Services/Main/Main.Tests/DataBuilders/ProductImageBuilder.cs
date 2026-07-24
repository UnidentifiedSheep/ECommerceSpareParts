using Bogus;
using Main.Entities.Product;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class ProductImageBuilder(Faker faker) : BuilderBase<ProductImage>(faker)
{
    private int _productId = 1;
    private string _extension = ".webp";

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

    public override ProductImage Build()
    {
        return ProductImage.Create(_productId, _extension);
    }
}
