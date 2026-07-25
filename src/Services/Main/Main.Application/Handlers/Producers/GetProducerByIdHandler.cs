using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Producer;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers;

public record GetProducerByIdQuery(int Id) : IQuery<GetProducerByIdResult>;

public record GetProducerByIdResult(ProducerDto Producer);

public class GetProducerByIdHandler(
    IReadRepository<Producer, int> repository,
    IProjectionProvider<Producer, ProducerDto> projection)
    : IQueryHandler<GetProducerByIdQuery, GetProducerByIdResult>
{
    public async Task<GetProducerByIdResult> Handle(
        GetProducerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var producer = await repository.Query
                           .Project(projection)
                           .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                       ?? throw new ProducerNotFoundException(request.Id);
        return new GetProducerByIdResult(producer);
    }
}
