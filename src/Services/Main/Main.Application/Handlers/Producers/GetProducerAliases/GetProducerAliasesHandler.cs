using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers.GetProducerAliases;

public record GetProducerAliasesQuery(int ProducerId, Pagination Pagination)
    : IQuery<GetProducerAliasesResult>;

public record GetProducerAliasesResult(IReadOnlyList<ProducerAliasDto> Aliases);

public class GetProducerAliasesHandler(IReadRepository<ProducerAlias, string> repository)
    : IQueryHandler<GetProducerAliasesQuery, GetProducerAliasesResult>
{
    public async Task<GetProducerAliasesResult> Handle(
        GetProducerAliasesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.ProducerId == request.ProducerId)
            .AsExpandable()
            .Select(ProducerProjections.ToAliasDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        return new GetProducerAliasesResult(result);
    }
}