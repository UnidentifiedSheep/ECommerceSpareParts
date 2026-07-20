using System.Text.Json.Serialization;

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
    
    [JsonPropertyName("items")]
    public IReadOnlyList<MarketInfoItem> Items { get; }

    [JsonPropertyName("offerCount")]
    public required int OfferCount { get; init; }

    [JsonPropertyName("availableQuantity")]
    public required int AvailableQuantity { get; init; }

    [JsonPropertyName("hasMarket")]
    public bool HasMarket => OfferCount > 0;
}
