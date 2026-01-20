namespace Main.Abstractions.Models;

public class LogisticsContext
{
    public decimal PriceKg { get; }
    public decimal PriceM3 { get; }
    public decimal PricePerOrder { get; }
    
    public decimal WightKg { get; }
    public decimal AreaM3 { get; }
    //TODO: add multiplier in future.
    public decimal Multiplier { get; }
    public decimal? MinimumPrice { get; }
    
    public LogisticsContext(decimal priceKg, decimal priceM3, decimal pricePerOrder, 
        int wightKg, int areaM3, decimal multiplier = 1m, decimal? minimumPrice = null)
    {
        PriceKg = priceKg;
        PriceM3 = priceM3;
        PricePerOrder = pricePerOrder;
        WightKg = wightKg;
        AreaM3 = areaM3;
        MinimumPrice = minimumPrice;
        Multiplier = multiplier;
    }
}