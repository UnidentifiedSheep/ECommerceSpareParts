using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Organizations;
using Main.Entities.Exceptions;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Organizations;

public record GetOrganizationQuery : IQuery<GetOrganizationResult>
{

	public GetOrganizationQuery(string systemName)
	{
		SystemName = Organization.NormalizeSystemName(systemName);
	}

	public GetOrganizationQuery(Guid id)
	{
		Id = id;
	}

	public Guid? Id { get; }

	public string? SystemName { get; }
}

public record GetOrganizationResult(OrganizationDto Organization);

public class GetOrganizationHandler(
	IReadRepository<Organization, Guid> repository,
	IProjectionProvider<Organization, OrganizationDto> projection)
	: IQueryHandler<GetOrganizationQuery, GetOrganizationResult>
{
	public async Task<GetOrganizationResult> Handle(
		GetOrganizationQuery request,
		CancellationToken cancellationToken)
	{
		var query = request.Id.HasValue
			? repository.Query.Where(x => x.Id == request.Id.Value)
			: repository.Query.Where(x => x.SystemName == request.SystemName!);

		var org = await query
			.Where(x => x.Type != OrganizationType.System)
			.Project(projection)
			.FirstOrDefaultAsync(cancellationToken);

		if (org != null)
			return new GetOrganizationResult(org);

		throw request.Id.HasValue
			? new OrganizationNotFoundException(request.Id.Value)
			: new OrganizationNotFoundException(request.SystemName!);
	}
}
