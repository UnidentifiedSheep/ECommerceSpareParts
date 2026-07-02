using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers;

public record GetFullProducersQuery : IQuery<GetFullProducersResult>
{
    public IReadOnlyList<int> Ids { get; }
    public GetFullProducersQuery(IEnumerable<int> ids)
    {
        Ids = ids.Distinct().ToList();
    }
    
    public GetFullProducersQuery(int id) : this([id]) {}
}

public record GetFullProducersResult(IReadOnlyList<ProducerFullDto> Producers);

public class GetFullProducersHandler(
    IReadRepository<Producer, int> repository
) : IQueryHandler<GetFullProducersQuery, GetFullProducersResult>
{
    public async Task<GetFullProducersResult> Handle(
        GetFullProducersQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0) return new GetFullProducersResult([]);

        var result = await repository
            .Query
            .Where(x => request.Ids.Contains(x.Id))
            .AsExpandable()
            .Select(ProducerProjections.ToFullDto)
            .ToListAsync(cancellationToken);
        
        return new GetFullProducersResult(result);
    }
}