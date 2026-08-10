using Abstractions.Interfaces.Persistence;
using Abstractions.Models;
using Application.Common.Services.Events;
using Domain.Interfaces.Events;
using FluentAssertions;
using MediatR;
using Moq;

namespace Tests.Tests.Services.Events;

public sealed class DomainEventExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_PublishesEventsAfterActionCompletes()
    {
        var calls = new List<string>();
        var publisher = CreatePublisher((_, _) => calls.Add("publish"));
        var eventScope = new DomainEventScope(publisher.Object);
        var executor = new DomainEventExecutor(eventScope, publisher.Object);

        await executor.ExecuteAsync(() =>
        {
            eventScope.IsCollectionEnabled.Should().BeTrue();
            eventScope.Add(new TestDomainEvent(1));
            calls.Add("action");
            return Task.CompletedTask;
        });

        calls.Should().Equal("action", "publish");
        eventScope.IsCollectionEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_HandlerAddsEvent_PublishesNextRound()
    {
        var publishedValues = new List<int>();
        var eventScope = default(DomainEventScope)!;
        var publisher = CreatePublisher((notification, _) =>
        {
            var @event = notification.Should().BeOfType<TestDomainEvent>().Subject;
            publishedValues.Add(@event.Value);

            if (@event.Value == 1)
                eventScope.Add(new TestDomainEvent(2));
        });
        eventScope = new DomainEventScope(publisher.Object);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Context)
            .Returns(new UnitOfWorkContext());
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var executor = new DomainEventExecutor(
            eventScope,
            publisher.Object,
            unitOfWork.Object);

        await executor.ExecuteAsync(() =>
        {
            eventScope.Add(new TestDomainEvent(1));
            return Task.CompletedTask;
        });

        publishedValues.Should().Equal(1, 2);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_ActionFails_DiscardsCollectedEvents()
    {
        var publisher = CreatePublisher((_, _) => { });
        var eventScope = new DomainEventScope(publisher.Object);
        var executor = new DomainEventExecutor(eventScope, publisher.Object);

        var action = () => executor.ExecuteAsync(() =>
        {
            eventScope.Add(new TestDomainEvent(1));
            throw new InvalidOperationException("failed");
        });

        await action.Should().ThrowAsync<InvalidOperationException>();
        eventScope.Flush().Should().BeEmpty();
        publisher.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<IPublisher> CreatePublisher(
        Action<object, CancellationToken> callback)
    {
        var publisher = new Mock<IPublisher>();
        publisher
            .Setup(x => x.Publish(
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Callback(callback)
            .Returns(Task.CompletedTask);
        return publisher;
    }

    private sealed record TestDomainEvent(int Value) : IDomainEvent;
}
