using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Organizations;
using Main.Entities.Exceptions;
using Main.Entities.Organization;

namespace Main.Application.Handlers.Organizations.UpdateOrganization;

[Diagnostics]
[Transactional, AutoSave]
public record UpdateOrganizationCommand(
    Guid OrganizationId,
    PatchOrganizationDto Organization) : ICommand<UpdateOrganizationResult>;

public record UpdateOrganizationResult(Guid OrganizationId);

public class UpdateOrganizationHandler(
    IRepository<Organization, Guid> repository)
    : ICommandHandler<UpdateOrganizationCommand, UpdateOrganizationResult>
{
    public async Task<UpdateOrganizationResult> Handle(
        UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var org = await repository.GetById(request.OrganizationId, cancellationToken)
                  ?? throw new OrganizationNotFoundException(request.OrganizationId);

        request.Organization.Name.Apply(org.SetName);

        return new UpdateOrganizationResult(org.Id);
    }
}
