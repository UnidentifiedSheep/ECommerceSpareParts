using Main.Abstractions.Models.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Enums;

namespace Tests.ServicesTests.Logistics;

public class PerWeightPricingTests
{
    private readonly PerWeightPricing _strategy = new();

    [Fact]
    public void Calculate_ShouldReturnCorrectCost_BasedOnWeight()
    {
        var context = new LogisticsContext(priceKg: 10, priceM3: 0, pricePerOrder: 0);
        var items = new List<LogisticsItem>
        {
            new(Id: 1, Quantity: 2, Weight: 1.5m, WeightUnit: WeightUnit.Kilogram, AreaM3: 0.1m),
            new(Id: 2, Quantity: 1, Weight: 500, WeightUnit: WeightUnit.Gram, AreaM3: 0.05m)
        };

        var result = _strategy.Calculate(context, items);

        // Item 1: 1.5kg * 2 * 10 = 30
        // Item 2: 0.5kg * 1 * 10 = 5
        // Total: 35
        Assert.Equal(35m, result.TotalCost);
        Assert.Equal(3.5m, result.TotalWeight);
        Assert.Equal(LogisticPricingType.PerWeight, result.PricingModel);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(30m, result.Items[0].Cost);
        Assert.Equal(5m, result.Items[1].Cost);
    }

    [Fact]
    public void Calculate_ShouldHandleSkippedItems_WhenWeightIsZero()
    {
        var context = new LogisticsContext(priceKg: 10, priceM3: 0, pricePerOrder: 0);
        var items = new List<LogisticsItem>
        {
            new(Id: 1, Quantity: 1, Weight: 0, WeightUnit: WeightUnit.Kilogram, AreaM3: 0.1m)
        };

        var result = _strategy.Calculate(context, items);

        Assert.True(result.Items[0].Skipped);
        Assert.Contains("Вес должен быть больше 0", result.Items[0].Reason!);
        Assert.Equal(0m, result.TotalCost);
    }
}
