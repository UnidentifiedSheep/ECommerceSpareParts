using Main.Entities.Product;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders;

namespace Tests.TestContexts;

public class ProductTestContext(
    DContext context, 
    IMediator mediator,
    ProducerTestContext producerTestContext
    ) : TestContextBase<DContext>(context, mediator)
{
    public ProducerTestContext ProducerTestContext => producerTestContext;
    
    private readonly List<Product> _products = [];
    public IReadOnlyList<Product> Products => _products;
    
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _products.AddRange(await new ProductBuilder(Faker)
            .WithProducers(producerTestContext.Producers)
            .BuildManyAndAddToDb(DbContext, 10));
    }

    public static void Register(TestBase test)
    {
        test.RegisterBasicContext<ProducerTestContext>();
        test.RegisterBasicContext<ProductTestContext>();
    }
}