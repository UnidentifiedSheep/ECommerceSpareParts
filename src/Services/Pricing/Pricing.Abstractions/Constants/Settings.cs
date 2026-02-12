using Abstractions.Models;
using Pricing.Abstractions.Models.Settings;
using Pricing.Enums;

namespace Pricing.Abstractions.Constants;

public static class Settings
{
    public static readonly TypedSetting<PricingSettings> Pricing = 
        new("pricing", new PricingSettings(ArticlePricingType.Average, -1));
    
    public static readonly TypedSetting[] AllSettings =
    [
        Pricing
    ];
}
