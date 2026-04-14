using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Anonymous.Producers;
using Main.Entities.Producer;
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
        var searchTerm = request.SearchTerm;

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query
                .Where(x => EF.Functions.ILike(x.Name, $"%{searchTerm}%"))
                .OrderByDescending(x => EF.Functions.TrigramsSimilarity(x.Name, searchTerm));
        else
            query = query.OrderBy(x => x.Name);
        

        var result = await query
            .Select(x => new ProducerDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
            })
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetProducersResult(result);
    }
}