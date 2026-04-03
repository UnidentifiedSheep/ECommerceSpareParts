using Analytics.Application.Handlers.Metrics.CreateMetric;
using Analytics.Entities.Metrics;
using Analytics.Enums;
using Analytics.Integration.Tests.TestContexts;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;

namespace Analytics.Integration.Tests.HandlerTests.Metrics;

[Collection("Combined collection")]
public class CreateMetricTests : IAsyncLifetime
{
    private readonly CreateMetricTestContext _context;
    public CreateMetricTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<CreateMetricTestContext>();
    }
    
    public async Task InitializeAsync()
    {
        await _context.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Context.ClearDatabase();
    }

    [Fact]
    public async Task CreateMetric_WhenJsonPayloadIsValid_ShouldSucceed()
    {
        var value = new
        {
            CurrencyId = _context.Currency.Id,
            RangeStart = new DateTime(2020, 01, 01),
            RangeEnd = new DateTime(2020, 01, 31),
            ArticleId = 1236,
        };
        string json = _context.Serializer.Serialize(value);

        var createdBy = Guid.NewGuid();
        
        var act = () => _context.Mediator
            .Send(new CreateMetricCommand("ArticleSalesMetric", json, createdBy));

        await act.Should().NotThrowAsync();

        var metrics = await _context.Context.Metrics
            .OfType<ArticleSalesMetric>()
            .ToListAsync();
        metrics.Should().HaveCount(1);
        
        var metric = metrics.First();
        
        metric.CreatedBy.Should().Be(createdBy);
        metric.RangeStart.Should().Be(value.RangeStart);
        metric.RangeEnd.Should().Be(value.RangeEnd);
        metric.ArticleId.Should().Be(value.ArticleId);
        metric.RecalculatedAt.Should().NotBeNull();
        metric.Tags.Should().Be(RecalculationTags.None);
    }
    
    [Fact]
    public async Task CreateMetric_WhenJsonPayloadInvalid_ShouldFail()
    {
        string json = _context.Serializer.Serialize(new
        {
            CurrencyId = _context.Currency.Id,
            RangeStart = new DateTime(2020, 03, 01),
            RangeEnd = new DateTime(2020, 01, 31),
            ArticleId = 1236,
        });

        var act = () => _context.Mediator
            .Send(new CreateMetricCommand("ArticleSalesMetric", json, Guid.NewGuid()));

        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Fact]
    public async Task CreateMetric_WhenInvalidMetricSystemName_ShouldFail()
    {
        string json = _context.Serializer.Serialize(new
        {
            CurrencyId = _context.Currency.Id,
            RangeStart = new DateTime(2020, 01, 01),
            RangeEnd = new DateTime(2020, 01, 31),
            ArticleId = 1236,
        });

        var act = () => _context.Mediator
            .Send(new CreateMetricCommand("", json, Guid.NewGuid()));

        await act.Should().ThrowAsync<NotSupportedException>();
    }
}