namespace Pricing.Application.Models.Pricing.MarketInfo;

public sealed record MarketInfo
{
    public static readonly MarketInfo Empty = new([])
    {
        OfferCount = 0,
        AvailableQuantity = 0,
    };

    public MarketInfo(IEnumerable<MarketInfoItem> items)
    {
        Items = items.OrderByDescending(x => x.Score).ToList();
    }
    
    public IReadOnlyList<MarketInfoItem> Items { get; }

    public required int OfferCount { get; init; }
    public required int AvailableQuantity { get; init; }

    public bool HasMarket => OfferCount > 0;
}