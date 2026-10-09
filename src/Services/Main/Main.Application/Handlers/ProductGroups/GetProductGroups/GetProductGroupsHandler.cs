using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductGroups.GetProductGroups;

public record GetProductGroupsQuery(string? SearchTerm, string[]? SortBy, Pagination Pagination)
	: IQuery<GetProductGroupsResult>;

public record GetProductGroupsResult(IReadOnlyList<ProductGroupDto> Groups);

public class GetProductGroupsHandler(
	IReadRepository<ProductGroup, int> repository,
	IProjectionProvider<ProductGroup, ProductGroupDto> projection)
	: IQueryHandler<GetProductGroupsQuery, GetProductGroupsResult>
{
	public async Task<GetProductGroupsResult> Handle(
		GetProductGroupsQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query;
		var searchTerm = request.SearchTerm?.Trim();
		if (!string.IsNullOrWhiteSpace(searchTerm))
			query = query.Where(group => EF.Functions.ILike(group.Name, $"%{searchTerm}%"));

		var groups = await query
			.SortBy(request.SortBy)
			.ThenBy(group => group.Id)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetProductGroupsResult(groups);
	}
}
