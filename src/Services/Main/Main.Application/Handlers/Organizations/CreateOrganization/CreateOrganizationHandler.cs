using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Organizations;
using Main.Application.Projections;
using Main.Entities.Organization;

namespace Main.Application.Handlers.Organizations.CreateOrganization;

[Transactional, AutoSave]
[Diagnostics(maxExecutionTimeMs: 500)]
public record CreateOrganizationCommand(
    Guid OwnerId,
    string Name,
    string SystemName
) : ICommand<CreateOrganizationResult>;

public record CreateOrganizationResult(OrganizationDto Organization);

public class CreateOrganizationHandler(IUnitOfWork unitOfWork)
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

        return new CreateOrganizationResult(
            OrganizationProjections.ToDto.AsFunc()(organization));
    }
}
