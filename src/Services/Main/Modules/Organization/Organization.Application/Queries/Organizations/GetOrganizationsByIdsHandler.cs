using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Modules.Organization.Application.Dtos.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Organization.Application.Queries.Organizations;

public sealed record GetOrganizationsByIdsQuery : IQuery<GetOrganizationsByIdsResult>
{
	public GetOrganizationsByIdsQuery(Guid id) : this([id]) { }

	public GetOrganizationsByIdsQuery(IEnumerable<Guid> ids) => Ids = ids.Distinct().ToArray();

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetOrganizationsByIdsResult(
	IReadOnlyDictionary<Guid, OrganizationDto> Organizations);

public sealed class GetOrganizationsByIdsHandler(
	IReadRepository<Entities.Organization.Organization.Organization, Guid> repository,
	IProjectionProvider<Entities.Organization.Organization.Organization, OrganizationDto> projection)
	: IQueryHandler<GetOrganizationsByIdsQuery, GetOrganizationsByIdsResult>
{
	public async Task<GetOrganizationsByIdsResult> Handle(
		GetOrganizationsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetOrganizationsByIdsResult(new Dictionary<Guid, OrganizationDto>());

		var organizations = await repository.Query
			.Where(x => request.Ids.Contains(x.Id))
			.Project(projection)
			.ToDictionaryAsync(x => x.Id, cancellationToken);

		return new GetOrganizationsByIdsResult(organizations);
	}
}
