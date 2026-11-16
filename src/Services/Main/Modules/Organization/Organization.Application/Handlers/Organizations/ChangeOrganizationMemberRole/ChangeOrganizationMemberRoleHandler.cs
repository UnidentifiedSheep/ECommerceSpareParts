using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Organization.Enums.Organization;
using Main.Entities.Organization.Exceptions;
using MediatR;

namespace Main.Modules.Organization.Application.Handlers.Organizations.ChangeOrganizationMemberRole;

[Transactional]
[AutoSave]
public record ChangeOrganizationMemberRoleCommand(Guid OrganizationId, Guid UserId, OrganizationRole Role)
	: ICommand;

public class ChangeOrganizationMemberRoleHandler(
	IRepository<Entities.Organization.Organization.Organization, Guid> repository)
	: ICommandHandler<ChangeOrganizationMemberRoleCommand>
{
	public async Task<Unit> Handle(
		ChangeOrganizationMemberRoleCommand request,
		CancellationToken cancellationToken)
	{
		var organization = await GetOrganization(request.OrganizationId, cancellationToken);
		organization.ChangeMemberRole(request.UserId, request.Role);

		return Unit.Value;
	}

	private Task<Entities.Organization.Organization.Organization> GetOrganization(
		Guid organizationId,
		CancellationToken cancellationToken)
		=> repository.EnsureExistForUpdateAsync(
			organizationId,
			id => new OrganizationNotFoundException(id),
			Criteria<Entities.Organization.Organization.Organization>.New().Include(x => x.Members),
			cancellationToken);
}
