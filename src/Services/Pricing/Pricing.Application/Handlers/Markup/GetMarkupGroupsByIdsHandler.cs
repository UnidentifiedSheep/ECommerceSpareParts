using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Markup;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Markup;

public sealed record GetMarkupGroupsByIdsQuery : IQuery<GetMarkupGroupsByIdsResult>
{
	public GetMarkupGroupsByIdsQuery(int id) : this([id]) { }

	public GetMarkupGroupsByIdsQuery(IEnumerable<int> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<int> Ids { get; }
}

public sealed record GetMarkupGroupsByIdsResult(IReadOnlyDictionary<int, MarkupGroupDto> Groups);

public sealed class GetMarkupGroupsByIdsHandler(
	IReadRepository<MarkupGroup, int> repository,
	IProjectionProvider<MarkupGroup, MarkupGroupDto> projection)
	: IQueryHandler<GetMarkupGroupsByIdsQuery, GetMarkupGroupsByIdsResult>
{
	public async Task<GetMarkupGroupsByIdsResult> Handle(
		GetMarkupGroupsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetMarkupGroupsByIdsResult(new Dictionary<int, MarkupGroupDto>());

		var groups = await repository.Query
			.Where(group => request.Ids.Contains(group.Id))
			.Project(projection)
			.ToDictionaryAsync(group => group.Id, cancellationToken);

		return new GetMarkupGroupsByIdsResult(groups);
	}
}
