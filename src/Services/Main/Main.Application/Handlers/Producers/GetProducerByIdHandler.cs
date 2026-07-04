using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers;

public record GetProducerByIdQuery(int Id) : IQuery<GetProducerByIdResult>;

public record GetProducerByIdResult(ProducerDto Producer);

public class GetProducerByIdHandler(IReadRepository<Producer, int> repository)
    : IQueryHandler<GetProducerByIdQuery, GetProducerByIdResult>
{
    public async Task<GetProducerByIdResult> Handle(
        GetProducerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var producer = await repository.Query
                           .AsExpandable()
                           .Select(ProducerProjections.ToDto)
                           .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                       ?? throw new ProducerNotFoundException(request.Id);
        return new GetProducerByIdResult(producer);
    }
}