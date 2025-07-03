namespace MonoliteUnicorn.Services.Prices.PriceGenerator.Models;

public class SettingsModel
{
    public int DefaultCurrency { get; set; }
    public double DefaultMarkUp { get; set; }
    public int MaximumDaysOfPriceStorage { get; set; }
    public int SelectedMarkupId { get; set; }
}