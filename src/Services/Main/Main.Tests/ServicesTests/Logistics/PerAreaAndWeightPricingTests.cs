using Enums;
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
        var context = new LogisticsContext(10, 100, 0);
        var items = new List<LogisticsItem>
        {
            // (0.1m3 * 100) + (1kg * 10) = 10 + 10 = 20
            new(1, 1, 1, WeightUnit.Kilogram, 0.1m),
            // (0.05m3 * 2 * 100) + (0.5kg * 2 * 10) = 10 + 10 = 20
            new(2, 2, 500, WeightUnit.Gram, 0.05m)
        };

        var result = _strategy.Calculate(context, items);

        Assert.Equal(40m, result.TotalCost);
        Assert.Equal(0.2m, result.TotalAreaM3);
        Assert.Equal(2m, result.TotalWeight);
        Assert.Equal(LogisticPricingType.PerAreaAndWeight, result.PricingModel);
    }
}