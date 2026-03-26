using Bogus;
using Enums;
using Extensions;
using Main.Entities;

namespace Tests.MockData.DataFactories;

public static class ArticleFactory
{
    private static readonly Faker<Article> Faker = new Faker<Article>(Global.Locale)
        .RuleFor(x => x.ArticleNumber, f => f.Lorem.Letter(20))
        .RuleFor(x => x.ArticleName, f => f.Commerce.ProductName())
        .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
        .RuleFor(x => x.ProducerId, f => f.Random.Int(1))
        .RuleFor(x => x.Indicator, f => f.Commerce.Color())
        .RuleFor(x => x.IsOe, f => f.Random.Bool())
        .RuleFor(x => x.PackingUnit, f => f.Random.Int(1, 100))
        .RuleFor(x => x.TotalCount, _ => 0)
        .RuleFor(x => x.ArticleSize, f =>
        {
            var h = Math.Round(f.Random.Decimal(1, 100), 2);
            var w = Math.Round(f.Random.Decimal(1, 100), 2);
            var l = Math.Round(f.Random.Decimal(1, 100), 2);
            var unit = f.PickRandom<DimensionUnit>();

            return new ArticleSize
            {
                Height = h,
                Width = w,
                Length = l,
                Unit = unit,
                VolumeM3 = DimensionExtensions.ToCubicMeters(l, w, h, unit)
            };
        })
        .RuleFor(x => x.ArticleWeight, f => new ArticleWeight
        {
            Weight = Math.Round(f.Random.Decimal(1, 100), 2),
            Unit = f.PickRandom<WeightUnit>()
        })
        .FinishWith((_, x) => { x.NormalizedArticleNumber = x.ArticleNumber.ToNormalizedArticleNumber(); });

    public static List<Article> Create(int count, params int[] producerIds)
    {
        var faker = Faker.Clone();
        faker.RuleFor(x => x.ProducerId, f => f.PickRandom(producerIds));
        return faker.Generate(count);
    }
}