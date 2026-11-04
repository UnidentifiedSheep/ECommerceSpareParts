using Application.Common.Querying;
using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Sale;
using Main.Application.Extensions.QueryExtensions;
using Main.Application.Interfaces.Products;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales.GetProductSaleHistory;

public record GetProductSaleHistoryQuery(
	int ProductId,
	Pagination Pagination,
	string? StorageCode,
	Guid? OrganizationId,
	int? CurrencyId,
	IReadOnlyCollection<SortExpression> SortBy,
	bool ShowCrosses = false) : IQuery<GetProductSaleHistoryResult>;

public record GetProductSaleHistoryResult(IReadOnlyList<ProductSaleHistoryDto> History);

public class GetProductSaleHistoryHandler(
	IReadRepository<SaleContent, int> repository,
	IProjectionProvider<SaleContent, ProductSaleHistoryDto> projectionProvider,
	IProductProvider productProvider)
	: IQueryHandler<GetProductSaleHistoryQuery, GetProductSaleHistoryResult>
{
	public async Task<GetProductSaleHistoryResult> Handle(
		GetProductSaleHistoryQuery request,
		CancellationToken cancellationToken)
	{
		var productIds = new List<int> { request.ProductId };
		if (request.ShowCrosses)
			productIds.AddRange(await productProvider.GetProductCrossesAsync(request.ProductId, cancellationToken));

		var query = repository.Query
			.CompletedSales()
			.Where(x => productIds.Contains(x.ProductId));

		if (!string.IsNullOrWhiteSpace(request.StorageCode))
			query = query.Where(x => x.Sale.StorageCode == request.StorageCode);

		if (request.OrganizationId.HasValue)
			query = query.Where(x => x.Sale.OrganizationId == request.OrganizationId);

		if (request.CurrencyId.HasValue)
			query = query.Where(x => x.Sale.CurrencyId == request.CurrencyId);


		var result = await query
			.SortBy(request.SortBy)
			.ThenByDescending(x => x.Id)
			.Project(projectionProvider)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetProductSaleHistoryResult(result);
	}
}
