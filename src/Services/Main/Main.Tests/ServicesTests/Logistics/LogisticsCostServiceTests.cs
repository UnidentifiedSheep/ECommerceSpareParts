using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models.Logistics;
using Main.Application.Services.Logistics;
using Main.Enums;
using Moq;

namespace Tests.ServicesTests.Logistics;

public class LogisticsCostServiceTests
{
    private readonly Mock<ILogisticsPricingStrategy> _strategyMock;
    private readonly LogisticsCostService _service;

    public LogisticsCostServiceTests()
    {
        _strategyMock = new Mock<ILogisticsPricingStrategy>();
        _strategyMock.Setup(x => x.Type).Returns(LogisticPricingType.PerWeight);
        
        _service = new LogisticsCostService([_strategyMock.Object]);
    }

    [Fact]
    public void Calculate_ShouldReturnStrategyResult_WhenNoMinimumPrice()
    {
        var context = new LogisticsContext(0, 0, 0, minimumPrice: null);
        var strategyResult = new LogisticsCalcResult { TotalCost = 50m };
        
        _strategyMock.Setup(x => x.Calculate(context, new List<LogisticsItem>())).Returns(strategyResult);
        
        var result = _service.Calculate(LogisticPricingType.PerWeight, context, new List<LogisticsItem>());
        
        Assert.Equal(50m, result.TotalCost);
        Assert.False(result.MinimalPriceApplied);
    }

    [Fact]
    public void Calculate_ShouldApplyMinimumPrice_WhenCostIsLower()
    {
        var context = new LogisticsContext(0, 0, 0, minimumPrice: 100m);
        var strategyResult = new LogisticsCalcResult { TotalCost = 50m };
        
        _strategyMock.Setup(x => x.Calculate(context, new List<LogisticsItem>())).Returns(strategyResult);
        
        var result = _service.Calculate(LogisticPricingType.PerWeight, context, new List<LogisticsItem>());
        
        Assert.Equal(100m, result.TotalCost);
        Assert.True(result.MinimalPriceApplied);
    }

    [Fact]
    public void Calculate_ShouldNotApplyMinimumPrice_WhenCostIsHigher()
    {
        var context = new LogisticsContext(0, 0, 0, minimumPrice: 30m);
        var strategyResult = new LogisticsCalcResult { TotalCost = 50m };
        
        _strategyMock.Setup(x => x.Calculate(context, new List<LogisticsItem>())).Returns(strategyResult);
        
        var result = _service.Calculate(LogisticPricingType.PerWeight, context, new List<LogisticsItem>());
        
        Assert.Equal(50m, result.TotalCost);
        Assert.False(result.MinimalPriceApplied);
    }
}
