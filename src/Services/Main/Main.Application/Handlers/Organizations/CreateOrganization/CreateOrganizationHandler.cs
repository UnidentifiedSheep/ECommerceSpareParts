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
[Diagnostics(maxExecutionTimeMs: 500)]
public record CreateOrganizationCommand(
    Guid OwnerId,
    string Name,
    string SystemName
) : ICommand<CreateOrganizationResult>;

public record CreateOrganizationResult(OrganizationDto Organization);

public class CreateOrganizationHandler(
    IUnitOfWork unitOfWork,
    IReadRepository<Organization, Guid> organizationRepository,
    IProjectionProvider<Organization, OrganizationDto> projection)
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
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var dbValue = await organizationRepository.Query
            .Project(projection)
            .FirstAsync(x => x.Id == organization.Id, cancellationToken);
        return new CreateOrganizationResult(dbValue);
    }
}
