using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Organization.Exceptions;
using MediatR;

namespace Main.Modules.Organization.Application.Handlers.Organizations.RemoveOrganizationMember;

[AutoSave]
[Transactional]
public record RemoveOrganizationMemberCommand(Guid OrganizationId, Guid UserId) : ICommand;

public class RemoveOrganizationMemberHandler(
	IRepository<Entities.Organization.Organization.Organization, Guid> repository)
	: ICommandHandler<RemoveOrganizationMemberCommand>
{
	public async Task<Unit> Handle(
		RemoveOrganizationMemberCommand request,
		CancellationToken cancellationToken)
	{
		var organization = await repository.EnsureExistForUpdateAsync(
			request.OrganizationId,
			id => new OrganizationNotFoundException(id),
			Criteria<Entities.Organization.Organization.Organization>.New().Include(x => x.Members),
			cancellationToken);

		organization.RemoveMember(request.UserId);

		return Unit.Value;
	}
}
