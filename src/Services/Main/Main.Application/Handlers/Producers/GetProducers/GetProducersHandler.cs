using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers.GetProducers;

public record GetProducersQuery(string? SearchTerm, Pagination Pagination) : IQuery<GetProducersResult>;

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
            .AsExpandable()
            .Select(ProducerProjections.ToDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetProducersResult(result);
    }
}