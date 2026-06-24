using System.Text.Json;
using Analytics.Application.NamedObjects.Metrics;
using Analytics.Entities.Exceptions;
using Analytics.Entities.Metrics;
using FluentAssertions;

namespace Analytics.Integration.Tests.Metrics.Definitions;

public class ProductSalesMetricDefinitionTests
{
    private readonly ProductSalesMetricDefinition _definition = new();

    [Fact]
    public void InputType_ShouldBeProductSalesMetricInput()
    {
        _definition.InputType.Should().Be(typeof(ProductSalesMetricInput));
    }

    [Fact]
    public void CreateMetric_WhenInputIsValid_ShouldCreateProductSalesMetric()
    {
        var input = CreateInput();
        var json = JsonSerializer.Serialize(input);

        var metric = _definition.CreateMetric(json);

        metric.Should().BeOfType<ProductSalesMetric>();
        metric.ProductId.Should().Be(input.ProductId);
        metric.CurrencyId.Should().Be(input.CurrencyId);
        metric.RangeStart.Should().Be(input.RangeStart);
        metric.RangeEnd.Should().Be(input.RangeEnd);
    }

    [Fact]
    public void CreateMetric_WhenJsonIsInvalid_ShouldThrowMetricInvalidInputException()
    {
        var act = () => _definition.CreateMetric("{invalid-json");

        act.Should().Throw<MetricInvalidInputException>();
    }

    [Fact]
    public void CreateMetric_WhenRangeStartIsAfterRangeEnd_ShouldThrowMetricInvalidInputException()
    {
        var input = CreateInput(
            rangeStart: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            rangeEnd: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var json = JsonSerializer.Serialize(input);

        var act = () => _definition.CreateMetric(json);

        act.Should().Throw<MetricInvalidInputException>();
    }

    private static ProductSalesMetricInput CreateInput(
        DateTime? rangeStart = null,
        DateTime? rangeEnd = null)
    {
        return new ProductSalesMetricInput
        {
            ProductId = 10,
            CurrencyId = 1,
            RangeStart = rangeStart ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            RangeEnd = rangeEnd ?? new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}
