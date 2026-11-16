using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Querying;
using Main.Modules.Product.Abstractions;
using Main.Modules.Purchase.Application.Dtos.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Purchase.Application.Handlers.Purchases.GetPurchases;

public record GetPurchasesQuery(
	RangeModel<DateTime> DateRange,
	Pagination Pagination,
	IReadOnlyCollection<Guid> SupplierOrganizationIds,
	IReadOnlyCollection<int> CurrencyIds,
	IReadOnlyCollection<int> ProductIds,
	IReadOnlyCollection<SortExpression> SortBy,
	string? SearchTerm) : IQuery<GetPurchasesResult>;

public record GetPurchasesResult(IReadOnlyCollection<PurchaseDto> Purchases);

public class GetPurchasesHandler(
	IReadRepository<Entities.Purchase.Purchase.Purchase, Guid> repository,
	IProductSearch productSearch,
	IProjectionProvider<Entities.Purchase.Purchase.Purchase, PurchaseDto> projection)
	: IQueryHandler<GetPurchasesQuery, GetPurchasesResult>
{
	public async Task<GetPurchasesResult> Handle(
		GetPurchasesQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query;

		if (request.DateRange.Min.HasValue)
			query = query.Where(x => x.PurchaseDatetime >= request.DateRange.Min.Value);

		if (request.DateRange.Max.HasValue)
			query = query.Where(x => x.PurchaseDatetime <= request.DateRange.Max.Value);

		if (request.SupplierOrganizationIds.Any())
			query = query.Where(x => request.SupplierOrganizationIds.Contains(x.SupplierOrganizationId));

		if (request.CurrencyIds.Any())
			query = query.Where(x => request.CurrencyIds.Contains(x.CurrencyId));

		if (request.ProductIds.Any())
			query = query.Where(x => x.Contents.Any(z => request.ProductIds.Contains(z.ProductId)));

		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
			query = ApplySearchTerm(query, request.SearchTerm);

		var purchases = await query
			.SortBy(request.SortBy)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetPurchasesResult(purchases);
	}

	private IQueryable<Entities.Purchase.Purchase.Purchase> ApplySearchTerm(IQueryable<Entities.Purchase.Purchase.Purchase> query, string searchTerm)
	{
		var term = searchTerm.Trim();
		var pattern = LikePattern.Contains(term);
		var productIds = productSearch.MatchingIds(term);

		return query.Where(purchase =>
			EF.Functions.ILike(purchase.Comment!, pattern, "\\") ||
			purchase.Contents.Any(content =>
				EF.Functions.ILike(content.Comment!, pattern, "\\") ||
				productIds.Contains(content.ProductId)));
	}
}
