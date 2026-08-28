using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Organizations;
using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Organizations.CreateOrganization;

[Transactional, AutoSave]
public record CreateOrganizationCommand(
    Guid OwnerId,
    string Name,
    string SystemName
) : ICommand<CreateOrganizationResult>;

public record CreateOrganizationResult(Guid OrganizationId);

public class CreateOrganizationHandler(
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrganizationCommand, CreateOrganizationResult>
{
    public async Task<CreateOrganizationResult> Handle(
        CreateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var organization = Organization.CreateBusiness(
            request.Name,
            request.SystemName,
            request.OwnerId);

        await unitOfWork.AddAsync(organization, cancellationToken);
        
        return new CreateOrganizationResult(organization.Id);
    }
}
