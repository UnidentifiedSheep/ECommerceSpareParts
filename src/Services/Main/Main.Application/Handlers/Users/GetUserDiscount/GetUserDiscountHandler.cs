using Application.Common.Interfaces;
using Core.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.DbRepositories;

namespace Main.Application.Handlers.Users.GetUserDiscount;

public record GetUserDiscountQuery(Guid UserId) : IQuery<GetUserDiscountResult>;

public record GetUserDiscountResult(decimal? Discount);

public class GetUserDiscountHandler(IUsersCacheRepository cacheUserRepository, IUserRepository usersRepository)
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