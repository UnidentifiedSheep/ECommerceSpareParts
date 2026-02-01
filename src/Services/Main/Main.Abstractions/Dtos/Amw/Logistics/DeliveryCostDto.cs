using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Logistics;

public class DeliveryCostDto
{
    public List<DeliveryCostItemDto> Items { get; set; } = [];
    
    public decimal TotalAreaM3 { get; set; }
    public decimal TotalWeight { get; set; }
    public WeightUnit WeightUnit { get; set; }
    
    public decimal TotalCost { get; set; }
    public decimal MinimalPrice { get; set; }
    public bool MinimalPriceApplied { get; set; }
    public int CurrencyId { get; set; }
    public LogisticPricingType PricingModel { get; set; }
}