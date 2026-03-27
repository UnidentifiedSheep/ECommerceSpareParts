using Bogus;
using Main.Entities;

namespace Tests.MockData.DataFactories;

public static class StorageFactory
{
    private static readonly Faker<Storage> Faker = new Faker<Storage>(Global.Locale)
        .RuleFor(x => x.Location,
            f => f.Random.Bool() ? f.Address.FullAddress() : null)
        .RuleFor(x => x.Description,
            f => f.Random.Bool() ? f.Commerce.ProductDescription() : null)
        .RuleFor(x => x.Name,
            f => f.Company.CompanyName());

    public static List<Storage> Create(int count)
    {
        return Faker.Generate(count);
    }
}