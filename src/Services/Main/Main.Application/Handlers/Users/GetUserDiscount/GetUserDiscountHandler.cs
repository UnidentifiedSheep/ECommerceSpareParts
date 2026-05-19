using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Interfaces.Cache;

namespace Main.Application.Handlers.Users.GetUserDiscount;

public record GetUserDiscountQuery(Guid UserId) : IQuery<GetUserDiscountResult>;

public record GetUserDiscountResult(decimal? Discount);

public class GetUserDiscountHandler(
    IUserCacheRepository userCache)
    : IQueryHandler<GetUserDiscountQuery, GetUserDiscountResult>
{
    public async Task<GetUserDiscountResult> Handle(GetUserDiscountQuery request, CancellationToken cancellationToken)
    {
        var discount = await userCache.GetUserDiscountAsync(request.UserId, cancellationToken);
        return new GetUserDiscountResult(discount);
    }
}