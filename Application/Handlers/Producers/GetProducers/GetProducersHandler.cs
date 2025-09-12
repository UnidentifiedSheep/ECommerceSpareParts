using Application.Interfaces;
using Core.Dtos.Anonymous.Producers;
using Core.Interfaces.DbRepositories;
using Core.Models;
using Mapster;

namespace Application.Handlers.Producers.GetProducers;

public record GetProducersQuery(string? SearchTerm, PaginationModel Pagination) : IQuery<GetProducersResult>;
public record GetProducersResult(IEnumerable<ProducerDto> Producers);

public class GetProducersHandler(IProducerRepository producerRepository) : IQueryHandler<GetProducersQuery, GetProducersResult>
{
    public async Task<GetProducersResult> Handle(GetProducersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        var result =await producerRepository.GetProducers(request.SearchTerm, page, size, false, cancellationToken);
        return new GetProducersResult(result.Adapt<List<ProducerDto>>());
    }
}