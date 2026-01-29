using Main.Abstractions.Models.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Enums;

namespace Tests.ServicesTests.Logistics;

public class PerOrderPricingTests
{
    private readonly PerOrderPricing _strategy = new();

    [Fact]
    public void Calculate_ShouldReturnOrderPrice_RegardlessOfItems()
    {
        var context = new LogisticsContext(priceKg: 10, priceM3: 100, pricePerOrder: 500);
        var items = new List<LogisticsItem>
        {
            new(Id: 1, Quantity: 10, Weight: 100, WeightUnit: WeightUnit.Kilogram, AreaM3: 10)
        };

        var result = _strategy.Calculate(context, items);

        Assert.Equal(500m, result.TotalCost);
        Assert.Equal(LogisticPricingType.PerOrder, result.PricingModel);
    }
}
