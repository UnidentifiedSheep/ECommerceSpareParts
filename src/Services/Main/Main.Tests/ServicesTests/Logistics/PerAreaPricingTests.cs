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
        var context = new LogisticsContext(priceKg: 0, priceM3: 100, pricePerOrder: 0);
        var items = new List<LogisticsItem>
        {
            new(Id: 1, Quantity: 2, Weight: 1, WeightUnit: WeightUnit.Kilogram, AreaM3: 0.1m), // 0.1 * 2 * 100 = 20
            new(Id: 2, Quantity: 1, Weight: 1, WeightUnit: WeightUnit.Kilogram, AreaM3: 0.05m) // 0.05 * 1 * 100 = 5
        };

        var result = _strategy.Calculate(context, items);

        Assert.Equal(25m, result.TotalCost);
        Assert.Equal(0.25m, result.TotalAreaM3);
        Assert.Equal(LogisticPricingType.PerArea, result.PricingModel);
    }
}
