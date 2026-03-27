using Enums;
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
        var context = new LogisticsContext(10, 100, 500);
        var items = new List<LogisticsItem>
        {
            new(1, 10, 100, WeightUnit.Kilogram, 10)
        };

        var result = _strategy.Calculate(context, items);

        Assert.Equal(500m, result.TotalCost);
        Assert.Equal(LogisticPricingType.PerOrder, result.PricingModel);
    }
}