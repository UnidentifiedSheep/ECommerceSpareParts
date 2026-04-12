using Main.Abstractions.Dtos.Currencies;
using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Purchase;

public class PurchaseLogisticDto
{
    public string PurchaseId { get; set; } = null!;
    public Guid RouteId { get; set; }
    public Guid? TransactionId { get; set; }
    public LogisticPricingType PricingModel { get; set; }
    public RouteType RouteType { get; set; }
    public decimal PriceKg { get; set; }
    public decimal PricePerM3 { get; set; }
    public decimal PricePerOrder { get; set; }
    public decimal? MinimumPrice { get; set; }
    public bool MinimumPriceApplied { get; set; }
    public CurrencyDto Currency { get; set; } = null!;
}