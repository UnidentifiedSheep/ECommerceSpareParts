using Abstractions.Interfaces.Services;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders;

namespace Tests.TestContexts.Base;

public class ProductTestContext(
    DContext context, 
    IMediator mediator,
    ProducerTestContext producerTestContext,
    IUnitOfWork unitOfWork) : TestContextBase<DContext>(context, mediator)
{
    public ProducerTestContext ProducerTestContext => producerTestContext;
    
    private readonly List<Product> _products = [];
    public IReadOnlyList<Product> Products => _products;
    
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await producerTestContext.InitializeAsync(cancellationToken);
        _products.AddRange(await new ProductBuilder(Faker)
            .WithProducers(producerTestContext.Producers)
            .BuildManyAndAddToDb(unitOfWork, 10));
    }
}