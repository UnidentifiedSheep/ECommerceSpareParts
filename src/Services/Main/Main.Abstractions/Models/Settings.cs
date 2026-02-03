using Main.Enums;

namespace Main.Abstractions.Models;

public class Settings
{
    public int DefaultCurrency { get; set; }
    public decimal DefaultMarkUp { get; set; }
    public int MaximumDaysOfPriceStorage { get; set; }
    public int SelectedMarkupId { get; set; }
    public ArticlePricingType PriceGenerationStrategy { get; set; } = ArticlePricingType.Average;
    public bool UseOrderAutoApprovement { get; set; }
}