using Main.Abstractions.Models.Pricing;

namespace Pricing.Abstractions.Models.Pricing;

public record BasePricingItem(int Id, IEnumerable<ArticlePrice> Prices, IEnumerable<PriceCoefficient> Coefficients);