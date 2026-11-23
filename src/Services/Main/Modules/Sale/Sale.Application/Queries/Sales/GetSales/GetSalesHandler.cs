using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Modules.Sale.Application.Dtos.Sale;
using Main.Modules.Sale.Application.Interfaces.Services;
using Main.Modules.Sale.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Sale.Application.Queries.Sales.GetSales;

public record GetSalesQuery(SalesFilter Filter, Pagination Pagination) : IQuery<GetSalesResult>;

public record GetSalesResult(IReadOnlyList<SaleDto> Sales);

public class GetSalesHandler(
	IReadRepository<Entities.Sale.Sale.Sale, Guid> repository,
	ISalesFilterApplier filterApplier,
	IProjectionProvider<Entities.Sale.Sale.Sale, SaleDto> projection)
	: IQueryHandler<GetSalesQuery, GetSalesResult>
{
	public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
	{
		var result = await filterApplier
			.Apply(repository.Query, request.Filter)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetSalesResult(result);
	}
}
