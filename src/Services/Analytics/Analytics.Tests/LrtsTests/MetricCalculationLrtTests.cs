using System.Text.Json;
using Analytics.Application.Lrts.MetricCalculation;
using Analytics.Entities.Metrics;
using Analytics.Integration.Tests.DataBuilders;
using Domain.CommonEnums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Analytics.Integration.Tests.LrtsTests;

public sealed class MetricCalculationLrtTests(CombinedContainerFixture fixture)
    : LrtIntegrationTest<MetricCalculationLrt>(fixture)
{
    [Fact]
    public async Task ProductSalesMetric_IsCalculatedAndPersisted()
    {
        var metric = await new ProductSalesMetricDataBuilder(Faker)
            .WithProductId(42)
            .BuildAndAddToDb(Context);

        var execution = await ExecuteLrt(
            JsonSerializer.Serialize(
                new MetricCalculationInputState { MetricId = metric.Id }));

        execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
        Context.ChangeTracker.Clear();
        var persisted = await Context.Metrics
            .OfType<ProductSalesMetric>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == metric.Id);
        persisted.RecalculatedAt.Should().NotBeNull();
        persisted.Data.Should().NotBeNull();
    }
}
