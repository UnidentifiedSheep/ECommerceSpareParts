using Main.Entities.Producer;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders;

namespace Tests.TestContexts;

public class ProducerTestContext(
    DContext context, 
    IMediator mediator
    ) : TestContextBase<DContext>(context, mediator)
{
    private readonly List<Producer> _producers = [];
    public IReadOnlyList<Producer> Producers => _producers;
    
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _producers.AddRange(await new ProducerBuilder(Faker)
            .BuildManyAndAddToDb(DbContext, 5));
    }
}