namespace Pricing.Application.Models.Pricing.MarketInfo;

public sealed record MarketInfo
{
    public static readonly MarketInfo Empty = new([])
    {
        MinCost = null,
        AverageCost = null,
        MaxCost = null,

        MinPrice = null,
        AveragePrice = null,
        MaxPrice = null,

        OfferCount = 0,
        AvailableQuantity = 0,
    };

    public MarketInfo(IEnumerable<MarketInfoItem> items)
    {
        Items = items.OrderByDescending(x => x.Score).ToList();
    }
    
    public IReadOnlyList<MarketInfoItem> Items { get; }
    public required decimal? MinCost { get; init; }
    public required decimal? AverageCost { get; init; }
    public required decimal? MaxCost { get; init; }
    
    public required decimal? MinPrice { get; init; }
    public required decimal? AveragePrice { get; init; }
    public required decimal? MaxPrice { get; init; }

    public required int OfferCount { get; init; }
    public required int AvailableQuantity { get; init; }

    public bool HasMarket => OfferCount > 0;
}