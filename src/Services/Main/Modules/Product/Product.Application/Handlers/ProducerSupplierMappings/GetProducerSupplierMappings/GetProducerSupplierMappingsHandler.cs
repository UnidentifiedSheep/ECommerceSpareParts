using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Enums;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Dtos.Producer.SupplierMappings;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Handlers.ProducerSupplierMappings.GetProducerSupplierMappings;

public record GetProducerSupplierMappingsQuery(
	int ProducerId,
	IReadOnlyCollection<Supplier> Suppliers,
	Pagination Pagination) : IQuery<GetProducerSupplierMappingsResult>;

public record GetProducerSupplierMappingsResult(IReadOnlyList<ProducerSupplierMappingDto> Mappings);

public class GetProducerSupplierMappingsHandler(
	IReadRepository<ProducerSupplierMapping, int> repository,
	IProjectionProvider<ProducerSupplierMapping, ProducerSupplierMappingDto> projection)
	: IQueryHandler<GetProducerSupplierMappingsQuery, GetProducerSupplierMappingsResult>
{
	public async Task<GetProducerSupplierMappingsResult> Handle(
		GetProducerSupplierMappingsQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query.Where(x => x.ProducerId == request.ProducerId);

		if (request.Suppliers.Any())
			query = query.Where(x => request.Suppliers.Contains(x.Supplier));

		var result = await query
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetProducerSupplierMappingsResult(result);
	}
}
