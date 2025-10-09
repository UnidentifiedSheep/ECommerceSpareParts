using Application.Common.Interfaces;
using Core.Interfaces.CacheRepositories;
using Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Users.ChangeUserDiscount;

public record ChangeUserDiscountCommand(Guid UserId, decimal Discount) : ICommand;

public class ChangeUserDiscountHandler(
    IRedisUserRepository cacheUserRepository,
    IUserRepository usersRepository) : ICommandHandler<ChangeUserDiscountCommand>
{
    public async Task<Unit> Handle(ChangeUserDiscountCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var discount = request.Discount;
        await usersRepository.ChangeUsersDiscount(userId, discount, cancellationToken);
        await cacheUserRepository.SetUserDiscount(userId, discount);
        return Unit.Value;
    }
}