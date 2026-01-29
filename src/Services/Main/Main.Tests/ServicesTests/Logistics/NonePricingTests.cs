using Main.Abstractions.Models.Logistics;
using Main.Application.Services.Logistics.PricingStrategies;
using Main.Enums;

namespace Tests.ServicesTests.Logistics;

public class NonePricingTests
{
    private readonly NonePricing _strategy = new();

    [Fact]
    public void Calculate_ShouldAlwaysReturnZeroCost()
    {
        var context = new LogisticsContext(priceKg: 10, priceM3: 100, pricePerOrder: 1000);
        var items = new List<LogisticsItem>
        {
            new(Id: 1, Quantity: 10, Weight: 100, WeightUnit: WeightUnit.Kilogram, AreaM3: 10)
        };

        var result = _strategy.Calculate(context, items);

        Assert.Equal(0m, result.TotalCost);
        Assert.Equal(LogisticPricingType.None, result.PricingModel);
        Assert.Single(result.Items);
        Assert.Equal(0m, result.Items[0].Cost);
    }
}
