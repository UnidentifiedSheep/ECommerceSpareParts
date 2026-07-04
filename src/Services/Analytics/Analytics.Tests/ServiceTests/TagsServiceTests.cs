using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Models;
using Analytics.Entities;
using Analytics.Enums;
using Analytics.Integration.Tests.DataBuilders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Analytics.Integration.Tests.ServiceTests;

public class TagsServiceTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    private ITagsService _tagsService = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _tagsService = Scope.ServiceProvider.GetRequiredService<ITagsService>();
    }

    [Fact]
    public async Task WhenNoMetrics_UpdatesNothing()
    {
        var act = () => _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = DateTime.UtcNow,
                PreviousFactDatetime = DateTime.UtcNow.AddYears(-100)
            });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task WhenPurchaseMetricContainsNewFactDatetime_MarksMetricDirty()
    {
        var start = DateTime.UtcNow.AddDays(-5);
        var end = DateTime.UtcNow.AddDays(5);

        var metric = await new ProductPurchasesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithStartDate(start)
            .WithEndDate(end)
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = DateTime.UtcNow,
                PreviousFactDatetime = DateTime.UtcNow.AddYears(-100)
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().HaveFlag(RecalculationTags.RecalculationNeeded);
    }

    [Fact]
    public async Task WhenPreviousFactDatetimeIsNullAndNewFactDatetimeIsInsideRange_MarksMetricDirty()
    {
        var factDatetime = DateTime.UtcNow;

        var metric = await new ProductPurchasesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithStartDate(factDatetime.AddDays(-1))
            .WithEndDate(factDatetime.AddDays(1))
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = factDatetime
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().HaveFlag(RecalculationTags.RecalculationNeeded);
    }

    [Fact]
    public async Task WhenPurchaseMetricDoesNotContainFactDatetimes_DoesNotMarkMetricDirty()
    {
        var metric = await new ProductPurchasesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithStartDate(DateTime.UtcNow.AddDays(-10))
            .WithEndDate(DateTime.UtcNow.AddDays(-5))
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = DateTime.UtcNow,
                PreviousFactDatetime = DateTime.UtcNow.AddDays(-20)
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().Be(RecalculationTags.None);
    }

    [Fact]
    public async Task WhenPreviousFactDatetimeIsInsideRange_MarksMetricDirty()
    {
        var previousFactDatetime = DateTime.UtcNow.AddDays(-7);

        var metric = await new ProductPurchasesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithStartDate(previousFactDatetime.AddDays(-1))
            .WithEndDate(previousFactDatetime.AddDays(1))
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = DateTime.UtcNow,
                PreviousFactDatetime = previousFactDatetime
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().HaveFlag(RecalculationTags.RecalculationNeeded);
    }

    [Fact]
    public async Task WhenNewFactDatetimeEqualsRangeStart_MarksMetricDirty()
    {
        var factDatetime = DateTime.UtcNow;

        var metric = await new ProductPurchasesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithStartDate(factDatetime)
            .WithEndDate(factDatetime.AddDays(1))
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = factDatetime
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().HaveFlag(RecalculationTags.RecalculationNeeded);
    }

    [Fact]
    public async Task WhenNewFactDatetimeEqualsRangeEnd_MarksMetricDirty()
    {
        var factDatetime = DateTime.UtcNow;

        var metric = await new ProductPurchasesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithStartDate(factDatetime.AddDays(-1))
            .WithEndDate(factDatetime)
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = factDatetime
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().HaveFlag(RecalculationTags.RecalculationNeeded);
    }

    [Fact]
    public async Task WhenMetricHasExistingTags_PreservesExistingTagsAndMarksMetricDirty()
    {
        var factDatetime = DateTime.UtcNow;

        var metric = await new ProductPurchasesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithRecalculationTags(RecalculationTags.Disabled)
            .WithStartDate(factDatetime.AddDays(-1))
            .WithEndDate(factDatetime.AddDays(1))
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = factDatetime
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().HaveFlag(RecalculationTags.Disabled);
        metric.Tags.Should().HaveFlag(RecalculationTags.RecalculationNeeded);
    }

    [Fact]
    public async Task WhenMetricDependsOnAnotherFact_DoesNotMarkMetricDirty()
    {
        var metric = await new ProductSalesMetricDataBuilder(Faker)
            .WithRecalculated(true)
            .WithStartDate(DateTime.UtcNow.AddDays(-5))
            .WithEndDate(DateTime.UtcNow.AddDays(5))
            .BuildAndAddToDb(Context);

        await _tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = DateTime.UtcNow
            });

        await Context.Entry(metric).ReloadAsync();
        metric.Tags.Should().Be(RecalculationTags.None);
    }
}