using Contracts.User;
using MassTransit;
using MediatR;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Application.Handlers.Discount.SetUserDiscount;

namespace Pricing.Application.Consumers;

public class UserDiscountChangedConsumer(IMediator mediator, IUserCacheRepository userCacheRepository) 
    : IConsumer<UserDiscountChangedEvent>
{
    public async Task Consume(ConsumeContext<UserDiscountChangedEvent> context)
    {
        var discount = await userCacheRepository.GetUserDiscount(context.Message.UserId);
        if (discount?.Timestamp >= context.Message.ChangedAt) return;

        await mediator.Send(new SetUserDiscountCommand(context.Message.UserId, context.Message.Discount));
    }
}