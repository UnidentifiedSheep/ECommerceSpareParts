using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Organizations;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Organizations;

public sealed record GetOrganizationsByIdsQuery : IQuery<GetOrganizationsByIdsResult>
{
	public GetOrganizationsByIdsQuery(Guid id) : this([id]) { }

	public GetOrganizationsByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetOrganizationsByIdsResult(
	IReadOnlyDictionary<Guid, OrganizationListItemDto> Organizations);

public sealed class GetOrganizationsByIdsHandler(
	IReadRepository<Organization, Guid> repository,
	IProjectionProvider<Organization, OrganizationListItemDto> projection)
	: IQueryHandler<GetOrganizationsByIdsQuery, GetOrganizationsByIdsResult>
{
	public async Task<GetOrganizationsByIdsResult> Handle(
		GetOrganizationsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetOrganizationsByIdsResult(new Dictionary<Guid, OrganizationListItemDto>());

		var organizations = await repository.Query
			.Where(x => request.Ids.Contains(x.Id) && x.Type != OrganizationType.System)
			.Project(projection)
			.ToDictionaryAsync(x => x.Id, cancellationToken);

		return new GetOrganizationsByIdsResult(organizations);
	}
}
