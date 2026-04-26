using Enums;
using Main.Enums;

namespace Main.Abstractions.Models.Logistics;

public class LogisticsCalcResult
{
    public decimal TotalCost { get; set; }
    public decimal TotalAreaM3 { get; set; }
    public decimal TotalWeight { get; set; }
    public WeightUnit WeightUnit { get; set; }
    public decimal MinimalPrice { get; set; }
    public bool MinimalPriceApplied { get; set; }
    public LogisticPricingType PricingModel { get; set; }
    public List<LogisticsCalcItemResult> Items { get; set; } = [];
}