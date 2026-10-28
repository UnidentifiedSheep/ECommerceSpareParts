using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Markup.GetMarkups;

public record GetMarkupsQuery(Pagination Pagination, string? SearchTerm = null) : IQuery<GetMarkupsResult>;

public record GetMarkupsResult(IReadOnlyList<MarkupGroupDto> Groups);

public class GetMarkupsHandler(
	IReadRepository<MarkupGroup, int> repository,
	IProjectionProvider<MarkupGroup, MarkupGroupDto> projection)
	: IQueryHandler<GetMarkupsQuery, GetMarkupsResult>
{
	public async Task<GetMarkupsResult> Handle(GetMarkupsQuery request, CancellationToken cancellationToken)
	{
		var query = repository.Query;
		var term = request.SearchTerm?.Trim();
		if (!string.IsNullOrEmpty(term))
		{
			var normalizedTerm = term.ToUpperInvariant();
			query = query.Where(group => group.Name != null && group.Name.ToUpper().Contains(normalizedTerm));
		}

		var result = await query
			.OrderBy(x => x.Id)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetMarkupsResult(result);
	}
}
