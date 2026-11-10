using Abstractions.Models;
using Application.Common.Interfaces.Persistence;
using Application.Common.Services.Events;
using Domain.Interfaces.Events;
using FluentAssertions;
using MediatR;
using Moq;
using Persistence;

namespace Tests.Tests.Services.Events;

public sealed class DomainEventExecutorTests
{
	[Fact]
	public async Task ExecuteAsync_PublishesEventsAfterActionCompletes()
	{
		var calls = new List<string>();
		var publisher = CreatePublisher((_, _) => calls.Add("publish"));
		var eventScope = new DomainEventScope();
		var executor = new DomainEventExecutor(eventScope, publisher.Object, Mock.Of<IDomainEventCollector>());

		await executor.ExecuteAsync(
			() =>
			{
				eventScope.IsCollectionEnabled.Should().BeTrue();
				eventScope.Add(new TestDomainEvent(1));
				calls.Add("action");
				return Task.CompletedTask;
			},
			TestContext.Current.CancellationToken);

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
		eventScope = new DomainEventScope();

		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork.SetupGet(x => x.Context).Returns(new UnitOfWorkContext());
		unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

		var executor = new DomainEventExecutor(
			eventScope,
			publisher.Object,
			Mock.Of<IDomainEventCollector>(),
			unitOfWork.Object);

		await executor.ExecuteAsync(
			() =>
			{
				eventScope.Add(new TestDomainEvent(1));
				return Task.CompletedTask;
			},
			TestContext.Current.CancellationToken);

		publishedValues.Should().Equal(1, 2);
		unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	[Fact]
	public async Task ExecuteAsync_ActionFails_DiscardsCollectedEvents()
	{
		var publisher = CreatePublisher((_, _) =>
		{
		});
		var eventScope = new DomainEventScope();
		var executor = new DomainEventExecutor(eventScope, publisher.Object, Mock.Of<IDomainEventCollector>());

		var action = () => executor.ExecuteAsync(() =>
		{
			eventScope.Add(new TestDomainEvent(1));
			throw new InvalidOperationException("failed");
		});

		await action.Should().ThrowAsync<InvalidOperationException>();
		eventScope.Flush().Should().BeEmpty();
		publisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task BeforeSave_DrainsOnlyItsPhaseWithoutSaving()
	{
		var scope = default(DomainEventScope)!;
		var values = new List<int>();
		var publisher = CreatePublisher((notification, _) =>
		{
			var item = notification.Should().BeOfType<BeforeEvent>().Subject;
			values.Add(item.Value);
			if (item.Value == 1) scope.Add(new BeforeEvent(2));
		});
		scope = new DomainEventScope();
		scope.Add(new BeforeEvent(1));
		scope.Add(new TestDomainEvent(3));
		var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
		var executor = new DomainEventExecutor(scope, publisher.Object, Mock.Of<IDomainEventCollector>(), unitOfWork.Object);

		await executor.DispatchBeforeSaveAsync(TestContext.Current.CancellationToken);

		values.Should().Equal(1, 2);
		scope.Flush().Should().ContainSingle().Which.Should().Be(new TestDomainEvent(3));
		unitOfWork.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task BeforeSave_EndlessChainFailsAndClearsPendingEvents()
	{
		var scope = default(DomainEventScope)!;
		var publisher = CreatePublisher((_, _) => scope.Add(new BeforeEvent(1)));
		scope = new DomainEventScope();
		scope.Add(new BeforeEvent(1));
		scope.Add(new TestDomainEvent(3));
		var executor = new DomainEventExecutor(scope, publisher.Object, Mock.Of<IDomainEventCollector>());

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			executor.DispatchBeforeSaveAsync(TestContext.Current.CancellationToken));

		scope.Flush().Should().BeEmpty();
	}

	[Fact]
	public async Task BeforeSave_BatchesAndDeduplicatesWithoutConsumingOrdinaryKeyedEvents()
	{
		var published = new List<object>();
		var publisher = CreatePublisher((item, _) => published.Add(item));
		var scope = new DomainEventScope();
		scope.Add(new KeyedBatchBeforeEvent(1));
		scope.Add(new KeyedBatchBeforeEvent(2));
		scope.Add(new OrdinaryKeyedEvent());
		var executor = new DomainEventExecutor(scope, publisher.Object, Mock.Of<IDomainEventCollector>());

		await executor.DispatchBeforeSaveAsync(TestContext.Current.CancellationToken);

		var batch = published.Should().ContainSingle().Subject.Should().BeOfType<Batch<KeyedBatchBeforeEvent>>().Subject;
		batch.Items.Should().Equal(new KeyedBatchBeforeEvent(2));
		scope.Flush().Should().ContainSingle().Which.Should().BeOfType<OrdinaryKeyedEvent>();
	}

	private sealed record KeyedBatchBeforeEvent(int Value) : IBeforeSaveDomainEvent, IBatchableDomainEvent, IKeyedDomainEvent
	{
		public string GetKey() => "batch-before";
	}

	private sealed record OrdinaryKeyedEvent : IKeyedDomainEvent
	{
		public string GetKey() => "ordinary";
	}

	private sealed record BeforeEvent(int Value) : IBeforeSaveDomainEvent;

	private static Mock<IPublisher> CreatePublisher(Action<object, CancellationToken> callback)
	{
		var publisher = new Mock<IPublisher>();
		publisher
			.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Callback(callback)
			.Returns(Task.CompletedTask);
		return publisher;
	}

	private sealed record TestDomainEvent(int Value) : IDomainEvent;
}
