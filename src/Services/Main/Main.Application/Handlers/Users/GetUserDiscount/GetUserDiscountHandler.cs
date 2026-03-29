using Application.Common.Interfaces;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;

namespace Main.Application.Handlers.Users.GetUserDiscount;

public record GetUserDiscountQuery(Guid UserId) : IQuery<GetUserDiscountResult>;

public record GetUserDiscountResult(decimal? Discount);

public class GetUserDiscountHandler(IUserService usersService)
    : IQueryHandler<GetUserDiscountQuery, GetUserDiscountResult>
{
    public async Task<GetUserDiscountResult> Handle(GetUserDiscountQuery request, CancellationToken cancellationToken)
    {
        var discount = await usersService.GetUserDiscountAsync(request.UserId, cancellationToken);
        return new GetUserDiscountResult(discount);
    }
}