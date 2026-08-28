using System.Diagnostics;
using System.Diagnostics.Metrics;
using Application.Common.Behaviors;
using Application.Common.Diagnostics;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Models;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Tests.Behaviors;

public sealed class MetricsBehaviorTests
{
    [Fact]
    public async Task Handle_Query_CreatesCqrsActivity()
    {
        await using var services = CreateServices();
        using var listener = CreateListener(out var activities);
        var behavior = CreateBehavior<TestQuery, string>(services);

        var response = await behavior.Handle(
            new TestQuery(),
            _ => Task.FromResult("response"),
            CancellationToken.None);

        response.Should().Be("response");
        var activity = activities.Should().ContainSingle().Subject;
        activity.DisplayName.Should().Be("cqrs TestQuery");
        activity.Kind.Should().Be(ActivityKind.Internal);
        activity.GetTagItem("cqrs.request.name").Should().Be("TestQuery");
        activity.GetTagItem("cqrs.request.type").Should().Be("query");
        activity.Status.Should().Be(ActivityStatusCode.Unset);
    }

    [Fact]
    public async Task Handle_Command_CreatesCommandActivity()
    {
        await using var services = CreateServices();
        using var listener = CreateListener(out var activities);
        var behavior = CreateBehavior<TestCommand, Unit>(services);

        await behavior.Handle(
            new TestCommand(),
            _ => Task.FromResult(Unit.Value),
            CancellationToken.None);

        activities.Should().ContainSingle()
            .Which.GetTagItem("cqrs.request.type").Should().Be("command");
    }

    [Fact]
    public async Task Handle_Exception_RecordsErrorAndRethrows()
    {
        await using var services = CreateServices();
        using var listener = CreateListener(out var activities);
        var behavior = CreateBehavior<TestQuery, string>(services);
        var expected = new InvalidOperationException("failure");

        var action = () => behavior.Handle(
            new TestQuery(),
            _ => Task.FromException<string>(expected),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .Where(exception => ReferenceEquals(exception, expected));

        var activity = activities.Should().ContainSingle().Subject;
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("failure");
        activity.Events.Should().ContainSingle(x => x.Name == "exception");
    }

    private static ServiceProvider CreateServices()
    {
        return new ServiceCollection()
            .AddMetrics()
            .BuildServiceProvider();
    }

    private static MetricsBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>(
        IServiceProvider services)
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        var metrics = new CqrsMetrics(services.GetRequiredService<IMeterFactory>());
        return new MetricsBehavior<TRequest, TResponse>(metrics);
    }

    private static ActivityListener CreateListener(out List<Activity> activities)
    {
        activities = [];
        var capturedActivities = activities;
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == CqrsDiagnostics.ActivitySourceName,
            Sample = (ref _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = capturedActivities.Add
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private sealed record TestQuery : IQuery<string>;
    private sealed record TestCommand : ICommand;
}
