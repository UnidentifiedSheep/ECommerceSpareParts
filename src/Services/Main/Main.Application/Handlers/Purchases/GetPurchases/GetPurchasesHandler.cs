using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Purchase;
using Main.Entities.Purchase;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases.GetPurchases;

public record GetPurchasesQuery(
	RangeModel<DateTime> DateRange,
	Pagination Pagination,
	IReadOnlyCollection<Guid> SupplierOrganizationIds,
	IReadOnlyCollection<int> CurrencyIds,
	IReadOnlyCollection<int> ProductIds,
	IReadOnlyCollection<string> SortBy,
	string? SearchTerm) : IQuery<GetPurchasesResult>;

public record GetPurchasesResult(IReadOnlyCollection<PurchaseDto> Purchases);

public class GetPurchasesHandler(
	IReadRepository<Purchase, Guid> repository,
	IProjectionProvider<Purchase, PurchaseDto> projection)
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

	private static IQueryable<Purchase> ApplySearchTerm(IQueryable<Purchase> query, string searchTerm)
	{
		var term = searchTerm.Trim();
		var pattern = $"%{EscapePattern(term)}%";
		var normalizedSku = Sku.ToNormalized(term);
		var skuPattern = $"%{normalizedSku}%";

		return query.Where(document =>
			EF.Functions.ILike(document.Comment!, pattern, "\\") ||
			document.Contents.Any(content =>
				EF.Functions.ILike(content.Comment!, pattern, "\\") ||
				EF.Functions.ILike(content.Product.Name.Value, pattern, "\\") ||
				normalizedSku.Length > 0 && EF.Functions.ILike(content.Product.Sku.NormalizedValue, skuPattern)));
	}

	private static string EscapePattern(string value) =>
		value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
