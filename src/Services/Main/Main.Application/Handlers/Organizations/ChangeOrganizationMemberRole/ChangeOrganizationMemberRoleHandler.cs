using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Organization;
using Main.Enums.Organization;
using MediatR;

namespace Main.Application.Handlers.Organizations.ChangeOrganizationMemberRole;

[Transactional, AutoSave]
[Diagnostics(maxExecutionTimeMs: 500)]
public record ChangeOrganizationMemberRoleCommand(
    Guid OrganizationId,
    Guid UserId,
    OrganizationRole Role
) : ICommand;

public class ChangeOrganizationMemberRoleHandler(IRepository<Organization, Guid> repository)
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

    private Task<Organization> GetOrganization(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return repository.EnsureExistForUpdateAsync(
            organizationId,
            id => new OrganizationNotFoundException(id),
            Criteria<Organization>.New().Include(x => x.Members),
            cancellationToken);
    }
}
