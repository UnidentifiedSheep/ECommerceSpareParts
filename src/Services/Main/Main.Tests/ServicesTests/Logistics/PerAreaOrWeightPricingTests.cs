using Main.Abstractions.Models.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Enums;

namespace Tests.ServicesTests.Logistics;

public class PerAreaOrWeightPricingTests
{
    private readonly PerAreaOrWeightPricing _strategy = new();

    [Fact]
    public void Calculate_ShouldReturnMaxOfAreaAndWeight()
    {
        var context = new LogisticsContext(priceKg: 10, priceM3: 100, pricePerOrder: 0);
        
        // Item 1: Weight cost (1kg * 10 = 10) vs Area cost (0.05m3 * 100 = 5). Max = 10.
        // Item 2: Weight cost (0.5kg * 10 = 5) vs Area cost (0.1m3 * 100 = 10). Max = 10.
        var items = new List<LogisticsItem>
        {
            new(Id: 1, Quantity: 1, Weight: 1, WeightUnit: WeightUnit.Kilogram, AreaM3: 0.05m),
            new(Id: 2, Quantity: 1, Weight: 0.5m, WeightUnit: WeightUnit.Kilogram, AreaM3: 0.1m)
        };
        
        var result = _strategy.Calculate(context, items);
        
        Assert.Equal(20m, result.TotalCost);
        Assert.Equal(LogisticPricingType.PerAreaOrWeight, result.PricingModel);
    }
}
