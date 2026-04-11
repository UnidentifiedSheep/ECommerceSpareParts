using Bogus;
using Main.Entities;
using Main.Entities.Producer;

namespace Tests.MockData.DataFactories;

public static class ProducerFactory
{
    private static readonly Faker<Producer> Faker = new Faker<Producer>()
        .RuleFor(x => x.Name, f => f.Company.CompanyName())
        .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
        .RuleFor(x => x.IsOe, f => f.Random.Bool());

    public static List<Producer> Create(int count)
    {
        return Faker.Generate(count);
    }
}