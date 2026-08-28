using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProducerAliases;

public record GetProducersAliasesQuery : IQuery<GetProducersAliasesResult>
{
    public IReadOnlyList<int> ProducerIds { get; }

    public GetProducersAliasesQuery(IEnumerable<int> producerIds)
    {
        ProducerIds = producerIds.Distinct().ToList();
    }

    public GetProducersAliasesQuery(int producerId) : this([producerId])
    { }
}

public record GetProducersAliasesResult(
    Dictionary<int, List<string>> ProducersAliases);

public class GetProducersAliasesHandler(
    IReadRepository<ProducerAlias, string> repository
    ) : IQueryHandler<GetProducersAliasesQuery, GetProducersAliasesResult>
{
    public async Task<GetProducersAliasesResult> Handle(
        GetProducersAliasesQuery request, 
        CancellationToken cancellationToken)
    {
        var aliases = (await repository.Query
            .Where(x => request.ProducerIds.Contains(x.ProducerId))
            .ToListAsync(cancellationToken))
            .GroupBy(x => x.ProducerId, x => x.Alias)
            .ToDictionary(
                x => x.Key,
                x => x.ToList());
        
        return new GetProducersAliasesResult(aliases);
    }
}