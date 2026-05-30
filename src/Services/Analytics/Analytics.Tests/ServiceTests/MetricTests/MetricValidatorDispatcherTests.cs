using System.Reflection;
using Analytics.Application.Extensions;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Services.Metrics.Validators;
using Analytics.Attributes;
using Analytics.Entities.Metrics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;

namespace Analytics.Integration.Tests.ServiceTests.MetricTests;

public class MetricValidatorDispatcherTests(CombinedContainerFixture fixture) 
    : IntegrationTest(fixture)
{
    private IMetricValidatorDispatcher _dispatcher = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _dispatcher = Scope.ServiceProvider.GetRequiredService<IMetricValidatorDispatcher>();
    }

    [Fact]
    public void GetValidator_AllValidatorsExist()
    {
        var result = MetricExtensions.GetAvailableMetrics();

        foreach (var (type, _) in result)
        {
            var validator = _dispatcher.GetValidator(type);
            validator.Should().NotBeNull();
        }
    }
}