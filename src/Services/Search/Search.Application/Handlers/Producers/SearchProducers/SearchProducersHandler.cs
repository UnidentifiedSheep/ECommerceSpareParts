using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Search.Application.Dtos.Producers;
using Search.Application.Interfaces.Producer;
using Search.Entities;

namespace Search.Application.Handlers.Producers.SearchProducers;

public record SearchProducersQuery(
    string? Query,
    Pagination Pagination
) : IQuery<SearchProducersResult>;

public record SearchProducersResult(IEnumerable<ProducerSearchDto> Producers);

public class SearchProducersHandler(
    IProducerRepository producerRepository,
    IProjectionProvider<Producer, ProducerSearchDto> projection
) : IQueryHandler<SearchProducersQuery, SearchProducersResult>
{
    public async Task<SearchProducersResult> Handle(
        SearchProducersQuery request,
        CancellationToken cancellationToken)
    {
        var producers = await producerRepository.Search(
            request.Query,
            request.Pagination,
            cancellationToken);

        return new SearchProducersResult(
            producers.Select(projection.ProjectionFunc));
    }
}
