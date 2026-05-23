using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Search.Application.Dtos.Producers;
using Search.Application.Interfaces.Producer;
using Search.Application.Mapping;

namespace Search.Application.Handlers.Producers.SearchProducers;

public record SearchProducersQuery(
    string? Query,
    Pagination Pagination) : IQuery<SearchProducersResult>;

public record SearchProducersResult(IEnumerable<ProducerSearchDto> Producers);

public class SearchProducersHandler(
    IProducerRepository producerRepository) : IQueryHandler<SearchProducersQuery, SearchProducersResult>
{
    public async Task<SearchProducersResult> Handle(
        SearchProducersQuery request,
        CancellationToken cancellationToken)
    {
        var producers = await producerRepository.Search(
            request.Query,
            request.Pagination,
            cancellationToken);

        return new SearchProducersResult(producers.Select(x => x.ToProducerSearchDto()));
    }
}
