using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencyById;

public class GetCurrencyByIdCachePolicy : ICachePolicy<GetCurrencyByIdQuery>
{
    public string GetCacheKey(GetCurrencyByIdQuery request)
        => $"currency:{request.Id}";

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string> Tags => ["currency"];
}