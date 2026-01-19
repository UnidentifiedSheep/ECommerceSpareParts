using Application.Common.Interfaces;
using Core.Models;
using Main.Abstractions.Dtos.Anonymous.Producers;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Producers.GetProducers;

public record GetProducersQuery(string? SearchTerm, PaginationModel Pagination) : IQuery<GetProducersResult>;

public record GetProducersResult(IEnumerable<ProducerDto> Producers);

public class GetProducersHandler(IProducerRepository producerRepository)
    : IQueryHandler<GetProducersQuery, GetProducersResult>
{
    public async Task<GetProducersResult> Handle(GetProducersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        var result = await producerRepository.GetProducers(request.SearchTerm, page, size, false, cancellationToken);
        return new GetProducersResult(result.Adapt<List<ProducerDto>>());
    }
}