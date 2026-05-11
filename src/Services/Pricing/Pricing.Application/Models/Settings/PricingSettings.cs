using Pricing.Enums;

namespace Pricing.Abstractions.Models.Settings;

public record PricingSettings(ArticlePricingType PricingStrategy, int SelectedMarkupId, decimal DefaultMarkup = 20);