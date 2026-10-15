using Bogus;
using Main.Entities.Product;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class CharacteristicBuilder(Faker faker) : BuilderBase<Characteristic>(faker)
{
	public string? Name { get; private set; }

	public CharacteristicBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public override Characteristic Build() => Characteristic.Create(Name ?? Faker.Commerce.ProductName());
}
