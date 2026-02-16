using Application.Common.Interfaces;
using MediatR;
using Pricing.Abstractions.Interfaces.CacheRepositories;

namespace Pricing.Application.Handlers.Discount.SetUserDiscount;

public record SetUserDiscountCommand(Guid UserId, decimal Discount) : ICommand;

public class SetUserDiscountHandler(IUserCacheRepository userCacheRepository) : ICommandHandler<SetUserDiscountCommand>
{
    public async Task<Unit> Handle(SetUserDiscountCommand request, CancellationToken cancellationToken)
    {
        await userCacheRepository.SetUserDiscount(request.UserId, request.Discount);
        return Unit.Value;
    }
}