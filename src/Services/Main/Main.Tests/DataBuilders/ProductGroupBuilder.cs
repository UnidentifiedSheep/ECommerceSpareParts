using Bogus;
using Main.Entities.Product;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class ProductGroupBuilder(Faker faker) : BuilderBase<ProductGroup>(faker)
{
	public string? Name { get; private set; }

	public ProductGroupBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public override ProductGroup Build() => ProductGroup.Create(Name ?? Faker.Commerce.ProductName());
}
