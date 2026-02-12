namespace Pricing.Abstractions.Models;

public class DetailedPriceModel
{
    public int Id { get; set; }
    public double MinPrice { get; set; }
    public double RecommendedPrice { get; set; }
    public double RecommendedPriceWithDiscount { get; set; }
    public double PriceInUsd { get; set; }
}