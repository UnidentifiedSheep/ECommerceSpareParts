using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers.GetFullProducer;

public record GetFullProducerQuery(int Id) : IQuery<GetFullProducerResult>;

public record GetFullProducerResult(ProducerDto Producer, IReadOnlyList<ProducerAliasDto> Aliases);

public class GetFullProducerHandler(
    IReadRepository<Producer, int> repository
) : IQueryHandler<GetFullProducerQuery, GetFullProducerResult>
{
    public async Task<GetFullProducerResult> Handle(
        GetFullProducerQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository
                         .Query
                         .Where(x => x.Id == request.Id)
                         .AsExpandable()
                         .Select(x => new
                         {
                             producer = ProducerProjections.ToDto.Invoke(x),
                             otherNames =
                                 x.Aliases.Select(z => ProducerProjections.ToAliasDto.Invoke(z))
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new ProducerNotFoundException(request.Id);

        return new GetFullProducerResult(result.producer, result.otherNames.ToList());
    }
}