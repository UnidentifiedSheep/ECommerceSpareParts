using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers.GetFullProducer;

public record GetFullProducerQuery : IQuery<GetFullProducerResult>
{
    public IReadOnlyList<int> Ids { get; }
    public GetFullProducerQuery(IEnumerable<int> ids)
    {
        Ids = ids.ToList();
    }
    
    public GetFullProducerQuery(int id) : this([id]) {}
}

public record GetFullProducerResult(IReadOnlyList<GetFullProducerResultItem> Producers);
public record GetFullProducerResultItem(ProducerDto Producer, IReadOnlyList<ProducerAliasDto> Aliases);

public class GetFullProducerHandler(
    IReadRepository<Producer, int> repository
) : IQueryHandler<GetFullProducerQuery, GetFullProducerResult>
{
    public async Task<GetFullProducerResult> Handle(
        GetFullProducerQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0) return new GetFullProducerResult([]);

        var result = await repository
            .Query
            .Where(x => request.Ids.Contains(x.Id))
            .AsExpandable()
            .Select(x => new
            {
                producer = ProducerProjections.ToDto.Invoke(x),
                otherNames =
                    x.Aliases.Select(z => ProducerProjections.ToAliasDto.Invoke(z))
            })
            .ToListAsync(cancellationToken);
        
        return new GetFullProducerResult(result
            .Select(x => new GetFullProducerResultItem(x.producer, x.otherNames.ToList()))
            .ToList());
    }
}