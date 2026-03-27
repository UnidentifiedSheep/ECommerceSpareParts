using Enums;
using Main.Abstractions.Models.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Enums;

namespace Tests.ServicesTests.Logistics;

public class PerAreaPricingTests
{
    private readonly PerAreaPricing _strategy = new();

    [Fact]
    public void Calculate_ShouldReturnCorrectCost_BasedOnArea()
    {
        var context = new LogisticsContext(0, 100, 0);
        var items = new List<LogisticsItem>
        {
            new(1, 2, 1, WeightUnit.Kilogram, 0.1m), // 0.1 * 2 * 100 = 20
            new(2, 1, 1, WeightUnit.Kilogram, 0.05m) // 0.05 * 1 * 100 = 5
        };

        var result = _strategy.Calculate(context, items);

        Assert.Equal(25m, result.TotalCost);
        Assert.Equal(0.25m, result.TotalAreaM3);
        Assert.Equal(LogisticPricingType.PerArea, result.PricingModel);
    }
}