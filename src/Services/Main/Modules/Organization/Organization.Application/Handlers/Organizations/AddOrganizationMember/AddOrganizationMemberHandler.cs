using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Organization.Enums.Organization;
using Main.Entities.Organization.Exceptions;
using MediatR;

namespace Main.Modules.Organization.Application.Handlers.Organizations.AddOrganizationMember;

[Transactional]
[AutoSave]
public record AddOrganizationMemberCommand(Guid OrganizationId, Guid UserId, OrganizationRole Role)
	: ICommand;

public class AddOrganizationMemberHandler(
	IRepository<Entities.Organization.Organization.Organization, Guid> repository)
	: ICommandHandler<AddOrganizationMemberCommand>
{
	public async Task<Unit> Handle(AddOrganizationMemberCommand request, CancellationToken cancellationToken)
	{
		var organization = await GetOrganization(request.OrganizationId, cancellationToken);
		organization.AddMember(request.UserId, request.Role);

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
