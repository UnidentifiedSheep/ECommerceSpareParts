using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Organizations;
using Main.Application.Projections;
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
    IReadRepository<Organization, Guid> organizationRepository)
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
            .AsExpandable()
            .Select(OrganizationProjections.ToDto)
            .FirstAsync(x => x.Id == organization.Id, cancellationToken);
        return new CreateOrganizationResult(dbValue);
    }
}
