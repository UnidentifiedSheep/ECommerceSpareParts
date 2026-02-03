using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models.Pricing;
using Main.Enums;

namespace Main.Application.Services.ArticlePricing;

public class BasePriceService(IBasePriceStrategyFactory basePriceFactory) : IBasePricesService
{
    public BasePricingResult CalculatePrices(BasePricingContext context)
    {
        List<BasePricingItemResult> pricingResults = [];
        ArticlePricingType pricingType = context.PricingType;
        
        foreach (var item in context.Items)
            pricingResults.Add(CalculatePrice(item, pricingType));
        
        return new BasePricingResult(pricingResults, pricingType);
    }
    
    public BasePricingItemResult CalculatePrice(BasePricingItem item, ArticlePricingType pricingType)
    {
        var basePriceStrategy = basePriceFactory.GetStrategy(pricingType);
        decimal basePrice = basePriceStrategy.GetPrice(item.Prices);
        var (finalPrice, appliedCoefficients) = ApplyCoefficients(basePrice, item.Coefficients);
        return new BasePricingItemResult(item.Id, basePrice, finalPrice, appliedCoefficients);
    }
    
    private (decimal finalPrice, List<PriceCoefficient> applied) ApplyCoefficients(decimal basePrice, 
        IEnumerable<PriceCoefficient> coefficients)
    {
        var appliedCoefficients = new List<PriceCoefficient>();
        var now = DateTime.UtcNow;
        
        var sortedCoefficients = coefficients
            .Where(c => c.ValidTill >= now)
            .OrderBy(c => (int)c.Type)
            .ThenBy(c => c.Order)
            .ToList();

        decimal multiplier = 1m;
        decimal percentOfBaseTotal = 0m;
        decimal additiveTotal = 0m;

        foreach (var coef in sortedCoefficients)
        {
            switch (coef.Type)
            {
                case CoefficientType.Multiplicative:
                    multiplier *= coef.Value;
                    break;

                case CoefficientType.PercentOfBase:
                    percentOfBaseTotal += coef.Value;
                    break;

                case CoefficientType.Additive:
                    additiveTotal += coef.Value;
                    break;
            }

            appliedCoefficients.Add(coef);
        }

        var finalPrice = basePrice * multiplier
                         + basePrice * percentOfBaseTotal
                         + additiveTotal;


        return (finalPrice, appliedCoefficients);
    }
}