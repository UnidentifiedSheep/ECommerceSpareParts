using Bogus;
using Main.Application.Dtos.Amw.Purchase;

namespace Tests.MockData.DataFactories.Purchase;

public static class NewPurchaseContentDtoFactory
{
    private static readonly Faker<NewPurchaseContentDto> Faker = new Faker<NewPurchaseContentDto>(Global.Locale)
        .RuleFor(x => x.ArticleId, f => f.Random.Int(1))
        .RuleFor(x => x.Count, f => f.Random.Int(1, 10))
        .RuleFor(x => x.Price, f => decimal.Parse(f.Commerce.Price()))
        .RuleFor(x => x.CalculateLogistics, f => f.Random.Bool());

    public static List<NewPurchaseContentDto> Create(int count)
    {
        return Faker.Generate(count);
    }

    public static List<NewPurchaseContentDto> Create(int count, IEnumerable<int> articleIds)
    {
        var clone = Faker.Clone()
            .RuleFor(x => x.ArticleId, f => f.PickRandom(articleIds));
        return clone.Generate(count);
    }
}