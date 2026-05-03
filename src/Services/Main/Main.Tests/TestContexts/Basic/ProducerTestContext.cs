using Abstractions.Interfaces.Services;
using Main.Entities.Producer;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders;

namespace Tests.TestContexts.Base;

public class ProducerTestContext(
    DContext context, 
    IMediator mediator,
    IUnitOfWork unitOfWork) : TestContextBase<DContext>(context, mediator)
{
    private readonly List<Producer> _producers = [];
    public IReadOnlyList<Producer> Producers => _producers;
    
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _producers.AddRange(await new ProducerBuilder(Faker)
            .BuildManyAndAddToDb(unitOfWork, 5));
    }
}