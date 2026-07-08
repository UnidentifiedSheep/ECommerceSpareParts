namespace Pricing.Application.Models.Pricing.MarketInfo;

public record MarketInfoItem
{
    public decimal Cost { get; init; }
    public TimeSpan DeliveryTime { get; init; }
    public decimal Score { get; init; }
}