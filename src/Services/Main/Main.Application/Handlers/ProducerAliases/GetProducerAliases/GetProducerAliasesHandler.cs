using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Producer.Aliases;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProducerAliases.GetProducerAliases;

public record GetProducerAliasesQuery(int ProducerId, Pagination Pagination)
	: IQuery<GetProducerAliasesResult>;

public record GetProducerAliasesResult(IReadOnlyList<ProducerAliasDto> Aliases);

public class GetProducerAliasesHandler(
	IReadRepository<ProducerAlias, string> repository,
	IProjectionProvider<ProducerAlias, ProducerAliasDto> projection)
	: IQueryHandler<GetProducerAliasesQuery, GetProducerAliasesResult>
{
	public async Task<GetProducerAliasesResult> Handle(
		GetProducerAliasesQuery request,
		CancellationToken cancellationToken)
	{
		var result = await repository
			.Query
			.Where(x => x.ProducerId == request.ProducerId)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);
		return new GetProducerAliasesResult(result);
	}
}
