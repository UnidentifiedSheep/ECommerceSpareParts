using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Markup.GetMarkupRanges;

public sealed record GetMarkupRangesQuery(int GroupId, Pagination Pagination) : IQuery<GetMarkupRangesResult>;

public sealed record GetMarkupRangesResult(IReadOnlyList<MarkupRangeDto> Ranges);

public sealed class GetMarkupRangesHandler(
	IReadRepository<MarkupGroup, int> repository,
	IProjectionProvider<MarkupRange, MarkupRangeDto> projection)
	: IQueryHandler<GetMarkupRangesQuery, GetMarkupRangesResult>
{
	public async Task<GetMarkupRangesResult> Handle(
		GetMarkupRangesQuery request,
		CancellationToken cancellationToken)
	{
		var ranges = await repository.Query
			.Where(group => group.Id == request.GroupId)
			.SelectMany(group => group.MarkupRanges)
			.OrderBy(range => range.RangeStart)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetMarkupRangesResult(ranges);
	}
}
