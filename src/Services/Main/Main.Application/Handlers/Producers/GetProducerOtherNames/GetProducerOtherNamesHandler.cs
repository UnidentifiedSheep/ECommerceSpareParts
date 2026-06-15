using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Producer;
using Main.Application.Projections;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Producers.GetProducerOtherNames;

public record GetProducerOtherNamesQuery(int ProducerId, Pagination Pagination)
    : IQuery<GetProducerOtherNamesResult>;

public record GetProducerOtherNamesResult(IReadOnlyList<ProducerOtherNameDto> Names);

public class GetProducerOtherNamesHandler(IReadRepository<ProducerOtherName, ProducerOtherNameKey> repository)
    : IQueryHandler<GetProducerOtherNamesQuery, GetProducerOtherNamesResult>
{
    public async Task<GetProducerOtherNamesResult> Handle(
        GetProducerOtherNamesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.ProducerId == request.ProducerId)
            .AsExpandable()
            .Select(ProducerProjections.ToOtherNameDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        return new GetProducerOtherNamesResult(result);
    }
}