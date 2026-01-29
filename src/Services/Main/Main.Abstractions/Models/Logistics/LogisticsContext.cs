using Main.Enums;

namespace Main.Abstractions.Models.Logistics;

public class LogisticsContext(decimal priceKg, decimal priceM3, decimal pricePerOrder, decimal? minimumPrice = null)
{
    public decimal PriceKg { get; } = priceKg;
    public decimal PriceM3 { get; } = priceM3;
    public decimal PricePerOrder { get; } = pricePerOrder;
    public decimal? MinimumPrice { get; } = minimumPrice;
}