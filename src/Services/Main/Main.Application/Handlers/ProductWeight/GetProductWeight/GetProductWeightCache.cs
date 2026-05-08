using Application.Common.Interfaces;

namespace Main.Application.Handlers.ProductWeight.GetProductWeight;

public class GetProductWeightCache : ICachePolicy<GetProductWeightQuery>
{
    public string GetCacheKey(GetProductWeightQuery request)
    {
        return $"product:{request.ProductId}:weight";
    }

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string> Tags => ["product"];
    public string? BaseTag => null;
}