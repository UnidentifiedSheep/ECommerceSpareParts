using Bogus;
using Enums;
using Extensions;
using Main.Entities;
using Main.Entities.Product;

namespace Tests.MockData.DataFactories;

public static class ArticleFactory
{
    private static readonly Faker<Product> Faker = new Faker<Product>(Global.Locale)
        .RuleFor(x => x.Sku, f => f.Lorem.Letter(20))
        .RuleFor(x => x.Name, f => f.Commerce.ProductName())
        .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
        .RuleFor(x => x.ProducerId, f => f.Random.Int(1))
        .RuleFor(x => x.Indicator, f => f.Commerce.Color())
        .RuleFor(x => x.IsOe, f => f.Random.Bool())
        .RuleFor(x => x.PackingUnit, f => f.Random.Int(1, 100))
        .RuleFor(x => x.Stock, _ => 0)
        .RuleFor(x => x.ProductSize, f =>
        {
            var h = Math.Round(f.Random.Decimal(1, 100), 2);
            var w = Math.Round(f.Random.Decimal(1, 100), 2);
            var l = Math.Round(f.Random.Decimal(1, 100), 2);
            var unit = f.PickRandom<DimensionUnit>();

            return new ProductId
            {
                Height = h,
                Width = w,
                Length = l,
                Unit = unit,
                VolumeM3 = DimensionExtensions.ToCubicMeters(l, w, h, unit)
            };
        })
        .RuleFor(x => x.ProductWeight, f => new ProductWeight
        {
            Weight = Math.Round(f.Random.Decimal(1, 100), 2),
            Unit = f.PickRandom<WeightUnit>()
        })
        .FinishWith((_, x) => { x.NormalizedSku = x.Sku.ToNormalizedArticleNumber(); });

    public static List<Product> Create(int count, params int[] producerIds)
    {
        var faker = Faker.Clone();
        faker.RuleFor(x => x.ProducerId, f => f.PickRandom(producerIds));
        return faker.Generate(count);
    }
}