using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Organization.Exceptions;
using Main.Modules.Organization.Application.Dtos.Organizations;

namespace Main.Modules.Organization.Application.Commands.Organizations.UpdateOrganization;

[Transactional]
[AutoSave]
public record UpdateOrganizationCommand(Guid OrganizationId, PatchOrganizationDto Organization)
	: ICommand<UpdateOrganizationResult>;

public record UpdateOrganizationResult(Guid OrganizationId);

public class UpdateOrganizationHandler(
	IRepository<Entities.Organization.Organization.Organization, Guid> repository)
	: ICommandHandler<UpdateOrganizationCommand, UpdateOrganizationResult>
{
	public async Task<UpdateOrganizationResult> Handle(
		UpdateOrganizationCommand request,
		CancellationToken cancellationToken)
	{
		var org = await repository.GetById(request.OrganizationId, cancellationToken) ??
			throw new OrganizationNotFoundException(request.OrganizationId);

		request.Organization.Name.Apply(org.SetName);
		request.Organization.IsHidden.Apply(org.SetIsHidden);

		return new UpdateOrganizationResult(org.Id);
	}
}
