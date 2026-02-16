using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.User;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.DbRepositories;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Users.ChangeUserDiscount;

[Transactional]
public record ChangeUserDiscountCommand(Guid UserId, decimal DiscountRate) : ICommand;

public class ChangeUserDiscountHandler(IUsersCacheRepository cacheUserRepository, IUserRepository usersRepository, 
    IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint) 
    : ICommandHandler<ChangeUserDiscountCommand>
{
    public async Task<Unit> Handle(ChangeUserDiscountCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var discount = request.DiscountRate;
        await usersRepository.ChangeUsersDiscount(userId, discount, cancellationToken);
        await cacheUserRepository.SetUserDiscount(userId, discount);

        await publishEndpoint.Publish(new UserDiscountChangedEvent
        {
            UserId = userId, 
            Discount = discount, 
            ChangedAt = DateTime.UtcNow
        }, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}