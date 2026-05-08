using Application.Common.Interfaces;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public class GetProductCrossesCachePolicy : ICachePolicy<GetProductCrossesQuery>
{
    public string GetCacheKey(GetProductCrossesQuery request)
    {
        return $"product:{request.ProductId}:crosses";
    }

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string> Tags => ["product-crosses", "product"];
    public string? BaseTag => null;
}