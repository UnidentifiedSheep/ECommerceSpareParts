using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Querying;
using Main.Entities.Sale.Sale;
using Main.Modules.Product.Abstractions;
using Main.Modules.Sale.Application.Dtos.Sale;
using Main.Modules.Sale.Application.Extensions;
using Main.Modules.Sale.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Sale.Application.Handlers.Sales.GetProductSaleHistory;

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
	IProductCrosses productCrosses,
	ISaleBuyCostCalculator buyCosts)
	: IQueryHandler<GetProductSaleHistoryQuery, GetProductSaleHistoryResult>
{
	public async Task<GetProductSaleHistoryResult> Handle(
		GetProductSaleHistoryQuery request,
		CancellationToken cancellationToken)
	{
		var productIds = new List<int> { request.ProductId };
		if (request.ShowCrosses)
			productIds.AddRange(
				await productCrosses.GetCrossIdsAsync(request.ProductId, cancellationToken));

		var query = repository.Query
			.CompletedSales()
			.Where(x => productIds.Contains(x.ProductId));

		if (!string.IsNullOrWhiteSpace(request.StorageCode))
			query = query.Where(x => x.Sale.StorageCode == request.StorageCode);

		if (request.OrganizationId.HasValue)
			query = query.Where(x => x.Sale.OrganizationId == request.OrganizationId);

		if (request.CurrencyId.HasValue)
			query = query.Where(x => x.Sale.CurrencyId == request.CurrencyId);


		var page = await query
			.SortBy(request.SortBy)
			.ThenByDescending(x => x.Id)
			.Project(projectionProvider)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		var costs = await buyCosts.CalculateAsync(page, cancellationToken);

		var history = page
			.Select(x => x with
			{
				AverageBuyPrice = x.Quantity > 0 ? costs[x.SaleContentId] / x.Quantity : 0m
			})
			.ToList();

		return new GetProductSaleHistoryResult(history);
	}
}
