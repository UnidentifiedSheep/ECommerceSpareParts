using Analytics.Application.Extensions;
using Analytics.Application.Interfaces.Services.Metrics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;

namespace Analytics.Integration.Tests.ServiceTests.MetricTests;

public class MetricConverterDispatcherTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    private IMetricConverterDispatcher _dispatcher = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _dispatcher = Scope.ServiceProvider.GetRequiredService<IMetricConverterDispatcher>();
    }
    
    [Fact]
    public void GetValidator_AllConvertersExist()
    {
        var result = MetricExtensions.GetAvailableMetrics();

        foreach (var (type, _) in result)
        {
            var validator = _dispatcher.GetConverter(type);
            validator.Should().NotBeNull();
        }
    }
}