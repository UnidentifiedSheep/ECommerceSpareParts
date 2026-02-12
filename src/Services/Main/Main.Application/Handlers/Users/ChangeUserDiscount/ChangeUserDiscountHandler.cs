using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Users.ChangeUserDiscount;

[Transactional]
public record ChangeUserDiscountCommand(Guid UserId, decimal DiscountRate) : ICommand;

public class ChangeUserDiscountHandler(IUsersCacheRepository cacheUserRepository, IUserRepository usersRepository) 
    : ICommandHandler<ChangeUserDiscountCommand>
{
    public async Task<Unit> Handle(ChangeUserDiscountCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var discount = request.DiscountRate;
        await usersRepository.ChangeUsersDiscount(userId, discount, cancellationToken);
        await cacheUserRepository.SetUserDiscount(userId, discount);
        return Unit.Value;
    }
}