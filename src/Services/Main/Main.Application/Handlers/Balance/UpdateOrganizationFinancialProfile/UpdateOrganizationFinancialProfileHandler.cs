using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Balances;
using Main.Entities.Balance;
using Main.Entities.Organization;

namespace Main.Application.Handlers.Balance.UpdateOrganizationFinancialProfile;

[AutoSave]
[Transactional(
    IsolationLevel.ReadCommitted,
    20,
    2)]
public record UpdateOrganizationFinancialProfileCommand(
    Guid OrganizationId,
    PatchOrganizationFinancialProfileDto Patch
) : ICommand<UpdateOrganizationFinancialProfileResult>;

public record UpdateOrganizationFinancialProfileResult(Guid OrganizationId);

public class UpdateOrganizationFinancialProfileHandler(
    IRepository<OrganizationFinancialProfile, Guid> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateOrganizationFinancialProfileCommand, UpdateOrganizationFinancialProfileResult>
{
    public async Task<UpdateOrganizationFinancialProfileResult> Handle(
        UpdateOrganizationFinancialProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetById(request.OrganizationId, cancellationToken);

        if (profile == null)
        {
            profile = OrganizationFinancialProfile.Create(request.OrganizationId);
            await unitOfWork.AddAsync(profile, cancellationToken);
        }

        request.Patch.MinimalAllowedBalance.Apply(x => profile.SetMinAllowedBalance(x));
        return new UpdateOrganizationFinancialProfileResult(request.OrganizationId);
    }
}
