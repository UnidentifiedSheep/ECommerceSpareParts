using Main.Entities.Product;
using Main.Persistence.Context;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders;

namespace Tests.TestContexts;

public class ProductCrossTestContext(
    DContext context,
    ProductTestContext productTestContext
)
    : TestContextBase<DContext>(context), IDependentTestContext
{
    private readonly List<ProductCross> _productCrosses = [];

    public ProductTestContext ProductTestContext => productTestContext;
    public IReadOnlyList<ProductCross> ProductCrosses => _productCrosses;

    public static Type[] DependsOn { get; } =
    [
        typeof(ProductTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var products = productTestContext.Products
            .Take(6)
            .ToList();

        var crosses = await new ProductCrossBuilder(Faker)
            .WithProductIds(
                products[0].Id,
                products[1].Id,
                products[2].Id,
                products[3].Id,
                products[4].Id)
            .BuildManyAndAddToDb(DbContext, 3);

        _productCrosses.AddRange(crosses);
    }
}