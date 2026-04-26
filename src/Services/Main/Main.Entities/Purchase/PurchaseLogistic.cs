using Main.Entities.Balance;
using Main.Enums;

namespace Main.Entities.Purchase;

public class PurchaseLogistic
{
    public Guid PurchaseId { get; set; }

    public Guid RouteId { get; set; }

    public int CurrencyId { get; set; }

    public Guid? TransactionId { get; set; }

    public LogisticPricingType PricingModel { get; set; }

    public RouteType RouteType { get; set; }

    public decimal PriceKg { get; set; }

    public decimal PricePerM3 { get; set; }

    public decimal PricePerOrder { get; set; }

    public decimal? MinimumPrice { get; set; }

    public bool MinimumPriceApplied { get; set; }

    public virtual Currency.Currency Currency { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}