using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Dtos.Producer.SupplierMappings;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Handlers.ProducerSupplierMappings;

public record GetProducersSupplierMappingsQuery(IReadOnlyCollection<int> ProducerIds)
	: IQuery<GetProducersSupplierMappingsResult>;

public record GetProducersSupplierMappingsResult(Dictionary<int, List<ProducerSupplierMappingDto>> Mappings);

public class GetProducersSupplierMappingsHandler(
	IReadRepository<ProducerSupplierMapping, int> repository,
	IProjectionProvider<ProducerSupplierMapping, ProducerSupplierMappingDto> projectionProvider)
	: IQueryHandler<GetProducersSupplierMappingsQuery, GetProducersSupplierMappingsResult>
{
	public async Task<GetProducersSupplierMappingsResult> Handle(
		GetProducersSupplierMappingsQuery request,
		CancellationToken cancellationToken)
	{
		var mappings =
			(await repository
				.Query
				.Where(x => request.ProducerIds.Contains(x.ProducerId))
				.Project(projectionProvider)
				.ToListAsync(cancellationToken))
			.GroupBy(x => x.ProducerId)
			.ToDictionary(x => x.Key, x => x.ToList());

		return new GetProducersSupplierMappingsResult(mappings);
	}
}
