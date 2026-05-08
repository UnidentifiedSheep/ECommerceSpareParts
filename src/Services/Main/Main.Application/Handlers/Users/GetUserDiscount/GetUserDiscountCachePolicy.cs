using Application.Common.Interfaces;

namespace Main.Application.Handlers.Users.GetUserDiscount;

public class GetUserDiscountCachePolicy : ICachePolicy<GetUserDiscountQuery>
{
    public string GetCacheKey(GetUserDiscountQuery request)
    {
        return $"user:{request.UserId}:discount";
    }

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string> Tags => ["user"];
    public string? BaseTag => null;
}