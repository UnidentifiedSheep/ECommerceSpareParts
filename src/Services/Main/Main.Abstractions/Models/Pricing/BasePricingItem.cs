namespace Main.Abstractions.Models.Pricing;

public record BasePricingItem(int Id, IEnumerable<ArticlePrice> Prices, IEnumerable<PriceCoefficient> Coefficients);