using Application.Common.Behaviors;
using Application.Common.Interfaces.Events;
using FluentAssertions;
using MediatR;

namespace Tests.Tests.Behaviors;

public sealed class DomainEventPublisherBehaviorTests
{
    [Fact]
    public async Task Handle_DelegatesExecutionToDomainEventExecutor()
    {
        var executor = new RecordingDomainEventExecutor();
        var behavior = new DomainEventPublisherBehavior<TestRequest, Unit>(
            executor);
        var handlerCalls = 0;

        var result = await behavior.Handle(
            new TestRequest(),
            _ =>
            {
                handlerCalls++;
                return Task.FromResult(Unit.Value);
            },
            CancellationToken.None);

        result.Should().Be(Unit.Value);
        handlerCalls.Should().Be(1);
        executor.Calls.Should().Be(1);
    }

    private sealed record TestRequest : IRequest<Unit>;

    private sealed class RecordingDomainEventExecutor
        : IDomainEventExecutor
    {
        public int Calls { get; private set; }

        public async Task ExecuteAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            await action();
        }

        public async Task<T> ExecuteAsync<T>(
            Func<Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return await action();
        }
    }
}
