namespace MonoliteUnicorn.Services.Prices.PriceGenerator.Models;

public class SettingsModel
{
    public int DefaultCurrency { get; set; }
    public double DefaultMarkUp { get; set; }
    public bool UseGeneratedMarkUp { get; set; }
    public int MaximumDaysOfPriceStorage { get; set; }
}