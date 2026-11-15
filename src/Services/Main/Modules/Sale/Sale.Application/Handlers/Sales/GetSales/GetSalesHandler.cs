using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Querying;
using Main.Application.Dtos.Sale;
using Main.Entities.Sale;
using Main.Enums;
using Main.Modules.Identity.Abstractions;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales.GetSales;

public record GetSalesQuery(
	RangeModel<DateTime> DateRange,
	Pagination Pagination,
	IReadOnlyCollection<Guid> OrganizationIds,
	IReadOnlyCollection<int> CurrencyIds,
	IReadOnlyCollection<int> ProductIds,
	IReadOnlyCollection<SaleState> States,
	IReadOnlyCollection<SortExpression> SortBy,
	string? SearchTerm) : IQuery<GetSalesResult>;

public record GetSalesResult(IReadOnlyList<SaleDto> Sales);

public class GetSalesHandler(
	IReadRepository<Sale, Guid> repository,
	IUserSearch userSearch,
	IProductSearch productSearch,
	IProjectionProvider<Sale, SaleDto> projection) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
	public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
	{
		var query = repository.Query;

		if (request.States.Any())
			query = query.Where(x => request.States.Contains(x.State));

		if (request.DateRange.Min.HasValue)
			query = query.Where(x => x.SaleDatetime >= request.DateRange.Min.Value);

		if (request.DateRange.Max.HasValue)
			query = query.Where(x => x.SaleDatetime <= request.DateRange.Max.Value);

		if (request.CurrencyIds.Any())
			query = query.Where(x => request.CurrencyIds.Contains(x.CurrencyId));

		if (request.OrganizationIds.Any())
			query = query.Where(x => request.OrganizationIds.Contains(x.OrganizationId));

		if (request.ProductIds.Any())
			query = query.Where(x => x.Contents.Any(z => request.ProductIds.Contains(z.ProductId)));

		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
			query = ApplySearchTerm(query, request.SearchTerm);

		var result = await query
			.SortBy(request.SortBy)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetSalesResult(result);
	}

	private IQueryable<Sale> ApplySearchTerm(IQueryable<Sale> query, string searchTerm)
	{
		var term = searchTerm.Trim();
		var pattern = LikePattern.Contains(term);
		var productIds = productSearch.MatchingIds(term);
		var userIds = userSearch.MatchingIds(term);

		return query.Where(sale =>
			EF.Functions.ILike(sale.Comment!, pattern, "\\") ||
			userIds.Contains(sale.UserId) ||
			sale.Contents.Any(content =>
				EF.Functions.ILike(content.Comment!, pattern, "\\") ||
				productIds.Contains(content.ProductId)));
	}
}
