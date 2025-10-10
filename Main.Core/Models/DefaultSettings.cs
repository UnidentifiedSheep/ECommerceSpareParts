using Main.Core.Enums;

namespace Main.Core.Models;

public class DefaultSettings
{
    public int DefaultCurrency { get; set; }
    public decimal MinimalMarkup { get; set; } = 25;
    public double DefaultMarkUp { get; set; }
    public int MaximumDaysOfPriceStorage { get; set; }
    public int SelectedMarkupId { get; set; }
    public PriceGenerationStrategy PriceGenerationStrategy { get; set; }
}