using Main.Entities.Product;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class ProductTestContext(
    DContext context,
    ProducerTestContext producerTestContext
) : TestContextBase<DContext>(context), IDependentTestContext
{
    private readonly List<Product> _products = [];
    public ProducerTestContext ProducerTestContext => producerTestContext;
    public IReadOnlyList<Product> Products => _products;

    public static Type[] DependsOn { get; } =
    [
        typeof(ProducerTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _products.AddRange(
            await new ProductBuilder(Faker)
                .WithProducers(producerTestContext.Producers)
                .BuildManyAndAddToDb(DbContext, 10));
    }
}