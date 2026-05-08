using Application.Common.Interfaces;

namespace Main.Application.Handlers.Currencies.GetCurrencyById;

public class GetCurrencyByIdCachePolicy : ICachePolicy<GetCurrencyByIdQuery>
{
    public string GetCacheKey(GetCurrencyByIdQuery request)
    {
        return $"currency:{request.Id}";
    }

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string> Tags => ["currency"];
    public string BaseTag => "currency";
}