using Abstractions.Models;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Anonymous.Producers;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities.Producer;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers.GetProducers;

public record GetProducersQuery(string? SearchTerm, PaginationModel Pagination) : IQuery<GetProducersResult>;

public record GetProducersResult(IEnumerable<ProducerDto> Producers);

public class GetProducersHandler(IReadRepository<Producer, int> repository)
    : IQueryHandler<GetProducersQuery, GetProducersResult>
{
    public async Task<GetProducersResult> Handle(GetProducersQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{searchTerm}%"));

        var queryWithRank = query.Select(z => new
            {
                Producer = z,
                Rank = string.IsNullOrWhiteSpace(searchTerm) ? 0 : EF.Functions.TrigramsSimilarity(z.Name, searchTerm)
            })
            .OrderByDescending(x => x.Rank);
        var result = await repository.Query
            
        
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        var result = await producerRepository.GetProducers(request.SearchTerm, page, size, false, cancellationToken);
        return new GetProducersResult(result.Adapt<List<ProducerDto>>());
    }
}