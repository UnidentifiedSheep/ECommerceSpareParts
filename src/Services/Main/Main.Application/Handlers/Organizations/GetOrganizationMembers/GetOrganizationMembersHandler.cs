using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Organizations;
using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Organizations.GetOrganizationMembers;

public record GetOrganizationMembersQuery(Guid OrganizationId, Pagination Pagination)
	: IQuery<GetOrganizationMembersResult>;

public record GetOrganizationMembersResult(IReadOnlyList<OrganizationMemberDto> Members);

public class GetOrganizationMembersHandler(
	IReadRepository<OrganizationMember, OrganizationMemberKey> repository,
	IProjectionProvider<OrganizationMember, OrganizationMemberDto> projection)
	: IQueryHandler<GetOrganizationMembersQuery, GetOrganizationMembersResult>
{
	public async Task<GetOrganizationMembersResult> Handle(
		GetOrganizationMembersQuery request,
		CancellationToken cancellationToken)
	{
		var members = await repository
			.Query
			.Where(x => x.OrganizationId == request.OrganizationId)
			.OrderBy(x => x.Role)
			.ThenBy(x => x.UserId)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetOrganizationMembersResult(members);
	}
}
