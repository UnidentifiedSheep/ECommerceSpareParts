using Main.Entities.Product;
using Main.Persistence.Context;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders;

namespace Tests.TestContexts;

public class ProductMeasurementsTestContext(
    DContext context,
    ProductTestContext productContext)
    : TestContextBase<DContext>(context), IDependentTestContext
{
    public IReadOnlyCollection<ProductSize> Sizes { get; private set; } = null!;
    public IReadOnlyCollection<ProductWeight> Weights { get; private set; } = null!;

    public static Type[] DependsOn { get; } =
    [
        typeof(ProductTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var sizeBuilders = new List<ProductSizeBuilder>();
        var weightsBuilder = new List<ProductWeightBuilder>();
        foreach (var product in productContext.Products)
        {
            sizeBuilders.Add(new ProductSizeBuilder(Faker)
                .WithProductId(product.Id));
            
            weightsBuilder.Add(new ProductWeightBuilder(Faker)
                .WithProductId(product.Id));
        }

        Sizes = await sizeBuilders.BuildManyCombinedAndAddToDb(
            DbContext, 
            1, 
            false);
        Weights = await weightsBuilder.BuildManyCombinedAndAddToDb(
            DbContext, 
            1, 
            false);
        
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
