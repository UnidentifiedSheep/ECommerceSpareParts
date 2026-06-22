using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Balances;
using Main.Application.Dtos.Users;
using Main.Application.Projections;
using Main.Entities.Balance;

namespace Main.Application.Handlers.Balance.UpdateUserFinancialProfile;

[Diagnostics(maxExecutionTimeMs: 200)]
[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record UpdateUserFinancialProfileCommand(
    Guid UserId, 
    PatchUserFinancialProfileDto Patch) : ICommand<UpdateUserFinancialProfileResult>;
public record UpdateUserFinancialProfileResult(UserFinancialProfileDto Profile);

public class UpdateUserFinancialProfileHandler(
    IRepository<UserFinancialProfile, Guid> repository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<UpdateUserFinancialProfileCommand, UpdateUserFinancialProfileResult>
{
    public async Task<UpdateUserFinancialProfileResult> Handle(
        UpdateUserFinancialProfileCommand request, 
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetById(request.UserId, cancellationToken);

        if (profile == null)
        {
            profile = UserFinancialProfile.Create(request.UserId);
            await unitOfWork.AddAsync(profile, cancellationToken);
        }
        
        request.Patch.MinimalAllowedBalance.Apply(x => profile.SetMinAllowedBalance(x));
        return new UpdateUserFinancialProfileResult(
            BalanceProjections.ToUserFinancialProfileDto.AsFunc()(profile));
    }
}