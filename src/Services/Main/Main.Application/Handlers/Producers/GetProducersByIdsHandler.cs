    using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Producer;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers;

public sealed record GetProducersByIdsQuery : IQuery<GetProducersByIdsResult>
{
    public GetProducersByIdsQuery(int id)
        : this([id])
    {
    }

    public GetProducersByIdsQuery(IEnumerable<int> ids)
    {
        Ids = ids.Distinct().ToArray();
    }

    public IReadOnlyList<int> Ids { get; }
}

public sealed record GetProducersByIdsResult(
    IReadOnlyDictionary<int, ProducerDto> Producers)
{
    public ProducerDto Producer => Producers.Values.Single();
}

public sealed class GetProducersByIdsHandler(
    IReadRepository<Producer, int> repository,
    IProjectionProvider<Producer, ProducerDto> projection)
    : IQueryHandler<GetProducersByIdsQuery, GetProducersByIdsResult>
{
    public async Task<GetProducersByIdsResult> Handle(
        GetProducersByIdsQuery request,
        CancellationToken cancellationToken)
    {
        var producers = await repository.Query
            .Where(x => request.Ids.Contains(x.Id))
            .Project(projection)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (request.Ids.Count == 1 &&
            !producers.ContainsKey(request.Ids[0]))
            throw new ProducerNotFoundException(request.Ids[0]);

        return new GetProducersByIdsResult(producers);
    }
}
