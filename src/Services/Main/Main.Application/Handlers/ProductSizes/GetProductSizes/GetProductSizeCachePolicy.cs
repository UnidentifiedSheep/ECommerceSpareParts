using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;

namespace Main.Application.Handlers.ProductSizes.GetProductSizes;

public class GetProductSizeCachePolicy : ICachePolicy<GetProductSizeQuery>
{
    public string GetCacheKey(GetProductSizeQuery request)
    {
        return $"product:{request.ProductId}:sizes";
    }

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string> Tags => ["product"];
    public string? BaseTag => null;
}