using Main.Abstractions.Models.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Enums;

namespace Tests.ServicesTests.Logistics;

public class PerAreaAndWeightPricingTests
{
    private readonly PerAreaAndWeight _strategy = new();

    [Fact]
    public void Calculate_ShouldReturnCorrectCost_BasedOnAreaAndWeight()
    {
        var context = new LogisticsContext(priceKg: 10, priceM3: 100, pricePerOrder: 0);
        var items = new List<LogisticsItem>
        {
            // (0.1m3 * 100) + (1kg * 10) = 10 + 10 = 20
            new(Id: 1, Quantity: 1, Weight: 1, WeightUnit: WeightUnit.Kilogram, AreaM3: 0.1m),
            // (0.05m3 * 2 * 100) + (0.5kg * 2 * 10) = 10 + 10 = 20
            new(Id: 2, Quantity: 2, Weight: 500, WeightUnit: WeightUnit.Gram, AreaM3: 0.05m)
        };

        var result = _strategy.Calculate(context, items);

        Assert.Equal(40m, result.TotalCost);
        Assert.Equal(0.2m, result.TotalAreaM3);
        Assert.Equal(2m, result.TotalWeight);
        Assert.Equal(LogisticPricingType.PerAreaAndWeight, result.PricingModel);
    }
}
