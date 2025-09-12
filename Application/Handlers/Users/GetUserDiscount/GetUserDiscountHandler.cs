using Application.Interfaces;
using Core.Interfaces.CacheRepositories;
using Core.Interfaces.DbRepositories;

namespace Application.Handlers.Users.GetUserDiscount;

public record GetUserDiscountQuery(string UserId) : IQuery<GetUserDiscountResult>;

public record GetUserDiscountResult(decimal? Discount);

public class GetUserDiscountHandler(IRedisUserRepository cacheUserRepository, IUsersRepository usersRepository)
    : IQueryHandler<GetUserDiscountQuery, GetUserDiscountResult>
{
    public async Task<GetUserDiscountResult> Handle(GetUserDiscountQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var redisDiscount = await cacheUserRepository.GetUserDiscount(userId);
        if (redisDiscount != null) return new GetUserDiscountResult(redisDiscount.Value);
        var dbDiscount = await usersRepository.GetUsersDiscountAsync(userId, cancellationToken);
        return new GetUserDiscountResult(dbDiscount);
    }
}