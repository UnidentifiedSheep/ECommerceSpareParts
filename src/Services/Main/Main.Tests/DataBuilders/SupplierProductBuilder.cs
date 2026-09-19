using Bogus;
using Enums;
using Main.Entities.Product.Enrichment;
using Main.Entities.Product.ValueObjects;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public sealed class SupplierProductBuilder(Faker faker) : BuilderBase<SupplierProduct>(faker)
{
	public string? Sku { get; private set; }

	public string? Producer { get; private set; }

	public Supplier? Supplier { get; private set; }

	public int NamesCount { get; private set; }

	public SupplierProductBuilder WithSku(string sku)
	{
		Sku = sku;
		return this;
	}

	public SupplierProductBuilder WithProducer(string producer)
	{
		Producer = producer;
		return this;
	}

	public SupplierProductBuilder WithSupplier(Supplier supplier)
	{
		Supplier = supplier;
		return this;
	}

	public SupplierProductBuilder WithNamesCount(int namesCount)
	{
		NamesCount = namesCount < 0 ? 0 : namesCount;
		return this;
	}

	public override SupplierProduct Build()
	{
		var product = SupplierProduct.Create(
			new Sku(Sku ?? Faker.Random.AlphaNumeric(12)),
			Producer ?? Faker.Company.CompanyName(),
			Supplier ?? Faker.PickRandom<Supplier>());

		for (var i = 0; i < NamesCount; i++)
			product.AddName(Faker.Lorem.Letter(40));

		return product;
	}
}
