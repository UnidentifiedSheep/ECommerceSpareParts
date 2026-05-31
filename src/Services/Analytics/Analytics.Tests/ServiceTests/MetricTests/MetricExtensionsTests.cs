using Analytics.Application.Extensions;
using Analytics.Entities.Metrics;
using FluentAssertions;

namespace Analytics.Integration.Tests.ServiceTests.MetricTests;

public class MetricExtensionsTests
{
    [Fact]
    public void GetAvailableMetrics_ReturnsMetricsWithCalculators()
    {
        var result = MetricExtensions.GetAvailableMetrics();

        result.Should().BeEquivalentTo([
            (typeof(ProductPurchasesMetric), nameof(ProductPurchasesMetric)),
            (typeof(ProductSalesMetric), nameof(ProductSalesMetric))
        ]);
    }

    [Fact]
    public void GetAvailableMetrics_DoesNotReturnDuplicateMetricTypes()
    {
        var result = MetricExtensions.GetAvailableMetrics();

        result.Select(x => x.Item1)
            .Should()
            .OnlyHaveUniqueItems();
    }
}
