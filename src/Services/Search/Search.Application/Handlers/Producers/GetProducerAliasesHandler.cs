using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Search.Application.Dtos.Producers;
using Search.Application.Interfaces.Producer;
using ProducerAliasEntity = Search.Entities.ProducerAlias;

namespace Search.Application.Handlers.Producers;

public record GetProducerAliasesQuery(int ProducerId) : IQuery<GetProducerAliasesResult>;

public record GetProducerAliasesResult(IEnumerable<ProducerAlias> Aliases);

public class GetProducerAliasesHandler(
	IProducerRepository producerRepository,
	IProjectionProvider<ProducerAliasEntity, ProducerAlias> projection)
	: IQueryHandler<GetProducerAliasesQuery, GetProducerAliasesResult>
{
	public async Task<GetProducerAliasesResult> Handle(
		GetProducerAliasesQuery request,
		CancellationToken cancellationToken)
	{
		var producer = await producerRepository.GetById(request.ProducerId, cancellationToken);

		return new GetProducerAliasesResult(producer?.Aliases.Select(projection.ProjectionFunc) ?? []);
	}
}
