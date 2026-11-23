using Application.Common.Extensions;
using Application.Common.Querying;
using Main.Modules.Identity.Abstractions.Interfaces;
using Main.Modules.Product.Abstractions;
using Main.Modules.Sale.Application.Interfaces.Services;
using Main.Modules.Sale.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Sale.Application.Services;

public sealed class SalesFilterApplier(IUserSearch userSearch, IProductSearch productSearch) : ISalesFilterApplier
{
	public IQueryable<Entities.Sale.Sale.Sale> Apply(
		IQueryable<Entities.Sale.Sale.Sale> query,
		SalesFilter filter)
	{
		if (filter.States.Count > 0)
			query = query.Where(x => filter.States.Contains(x.State));

		if (filter.DateRange.Min.HasValue)
			query = query.Where(x => x.SaleDatetime >= filter.DateRange.Min.Value);

		if (filter.DateRange.Max.HasValue)
			query = query.Where(x => x.SaleDatetime <= filter.DateRange.Max.Value);

		if (filter.CurrencyIds.Count > 0)
			query = query.Where(x => filter.CurrencyIds.Contains(x.CurrencyId));

		if (filter.OrganizationIds.Count > 0)
			query = query.Where(x => filter.OrganizationIds.Contains(x.OrganizationId));

		if (filter.ProductIds.Count > 0)
			query = query.Where(x => x.Contents.Any(z => filter.ProductIds.Contains(z.ProductId)));

		if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
			query = ApplySearchTerm(query, filter.SearchTerm);

		return query.SortBy(filter.SortBy);
	}

	private IQueryable<Entities.Sale.Sale.Sale> ApplySearchTerm(
		IQueryable<Entities.Sale.Sale.Sale> query,
		string searchTerm)
	{
		var term = searchTerm.Trim();
		var pattern = LikePattern.Contains(term);
		var productIds = productSearch.MatchingIds(term);
		var userIds = userSearch.MatchingIds(term);

		return query.Where(sale =>
			EF.Functions.ILike(
				sale.Comment!,
				pattern,
				"\\") ||
			userIds.Contains(sale.UserId) ||
			sale.Contents.Any(content =>
				EF.Functions.ILike(
					content.Comment!,
					pattern,
					"\\") ||
				productIds.Contains(content.ProductId)));
	}
}
