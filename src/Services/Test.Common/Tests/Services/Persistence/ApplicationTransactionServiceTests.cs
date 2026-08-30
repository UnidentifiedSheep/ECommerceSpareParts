using Abstractions.Interfaces.Persistence;
using Abstractions.Models;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Services.Events;
using Application.Common.Services.Persistence;
using Attributes;
using FluentAssertions;
using MassTransit;
using Moq;

namespace Tests.Tests.Services.Persistence;

public sealed class ApplicationTransactionServiceTests
{
	[Fact]
	public async Task ExecuteAsync_TransactionalAction_ExecutesCompleteBoundaryInOrder()
	{
		var calls = new List<string>();
		using var cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = cancellationTokenSource.Token;
		var unitOfWork = CreateUnitOfWork(calls);
		var integrationEventScope = new IntegrationEventScope();
		var domainEventExecutor = CreateDomainEventExecutor(
			calls,
			() => integrationEventScope.Add("from-domain-handler"));
		var publisher = CreatePublisher(calls);
		var context = Mock.Of<IApplicationTransactionContext>();
		var service = new ApplicationTransactionService(
			unitOfWork.Object,
			domainEventExecutor.Object,
			integrationEventScope,
			publisher.Object,
			context);

		await service.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			(actualContext, actualCancellationToken) =>
			{
				actualContext.Should().BeSameAs(context);
				actualCancellationToken.Should().Be(cancellationToken);
				calls.Add("action");
				integrationEventScope.Add("from-action");
				return Task.CompletedTask;
			},
			cancellationToken);

		calls
		.Should()
		.Equal(
			"transaction:start",
			"domain-events:start",
			"action",
			"domain-events:end",
			"integration-event:publish",
			"integration-event:publish",
			"save-changes",
			"transaction:end");
		integrationEventScope.Flush().Should().BeEmpty();
	}

	[Fact]
	public async Task ExecuteAsync_WithoutTransaction_DoesNotOpenTransaction()
	{
		var calls = new List<string>();
		var unitOfWork = CreateUnitOfWork(calls);
		var service = new ApplicationTransactionService(
			unitOfWork.Object,
			CreateDomainEventExecutor(calls).Object,
			new IntegrationEventScope(),
			CreatePublisher(calls).Object,
			Mock.Of<IApplicationTransactionContext>());

		var result = await service.ExecuteAsync(
			null,
			(_, _) =>
			{
				calls.Add("action");
				return Task.FromResult(42);
			});

		result.Should().Be(42);
		calls
		.Should()
		.Equal(
			"domain-events:start",
			"action",
			"domain-events:end");
		unitOfWork.Verify(
			x => x.ExecuteWithTransaction(
				It.IsAny<TransactionalAttribute>(),
				It.IsAny<Func<Task<int>>>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task ExecuteAsync_ActionFails_DiscardsIntegrationEvents()
	{
		var calls = new List<string>();
		var integrationEventScope = new IntegrationEventScope();
		var publisher = CreatePublisher(calls);
		var service = new ApplicationTransactionService(
			CreateUnitOfWork(calls).Object,
			CreateDomainEventExecutor(calls).Object,
			integrationEventScope,
			publisher.Object,
			Mock.Of<IApplicationTransactionContext>());

		var action = () => service.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			(_, _) =>
			{
				integrationEventScope.Add("discard-me");
				throw new InvalidOperationException("failed");
			});

		await action.Should().ThrowAsync<InvalidOperationException>();
		integrationEventScope.Flush().Should().BeEmpty();
		publisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task ExecuteAsync_RoutedIntegrationEvent_PublishesWithPipe()
	{
		var calls = new List<string>();
		var integrationEventScope = new IntegrationEventScope();
		var publisher = new Mock<IPublishEndpoint>();
		publisher
			.Setup(x => x.Publish(
				It.IsAny<object>(),
				It.IsAny<IPipe<PublishContext>>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);
		var service = new ApplicationTransactionService(
			CreateUnitOfWork(calls).Object,
			CreateDomainEventExecutor(calls).Object,
			integrationEventScope,
			publisher.Object,
			Mock.Of<IApplicationTransactionContext>());

		await service.ExecuteAsync(
			null,
			(_, _) =>
			{
				integrationEventScope.Add("routed", "test-service");
				return Task.CompletedTask;
			});

		publisher.Verify(
			x => x.Publish(
				It.Is<object>(message => Equals(message, "routed")),
				It.IsAny<IPipe<PublishContext>>(),
				It.IsAny<CancellationToken>()),
			Times.Once);
		publisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	private static Mock<IUnitOfWork> CreateUnitOfWork(List<string> calls)
	{
		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork.SetupGet(x => x.Context).Returns(new UnitOfWorkContext());
		unitOfWork
			.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.Callback(() => calls.Add("save-changes"))
			.Returns(Task.CompletedTask);
		unitOfWork
			.Setup(x => x.ExecuteWithTransaction(
				It.IsAny<TransactionalAttribute>(),
				It.IsAny<Func<Task>>(),
				It.IsAny<CancellationToken>()))
			.Returns<TransactionalAttribute, Func<Task>, CancellationToken>(async (
				_, action,
				_) =>
			{
				calls.Add("transaction:start");
				await action();
				calls.Add("transaction:end");
			});
		unitOfWork
			.Setup(x => x.ExecuteWithTransaction(
				It.IsAny<TransactionalAttribute>(),
				It.IsAny<Func<Task<bool>>>(),
				It.IsAny<CancellationToken>()))
			.Returns<TransactionalAttribute, Func<Task<bool>>, CancellationToken>(async (
				_, action,
				_) =>
			{
				calls.Add("transaction:start");
				var result = await action();
				calls.Add("transaction:end");
				return result;
			});
		unitOfWork
			.Setup(x => x.ExecuteWithTransaction(
				It.IsAny<TransactionalAttribute>(),
				It.IsAny<Func<Task<int>>>(),
				It.IsAny<CancellationToken>()))
			.Returns<TransactionalAttribute, Func<Task<int>>, CancellationToken>(async (
				_, action,
				_) =>
			{
				calls.Add("transaction:start");
				var result = await action();
				calls.Add("transaction:end");
				return result;
			});
		return unitOfWork;
	}

	private static Mock<IDomainEventExecutor> CreateDomainEventExecutor(
		List<string> calls,
		Action? afterAction = null)
	{
		var executor = new Mock<IDomainEventExecutor>();
		executor
			.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
			.Returns<Func<Task>, CancellationToken>(async (action, _) =>
			{
				calls.Add("domain-events:start");
				await action();
				afterAction?.Invoke();
				calls.Add("domain-events:end");
			});
		executor
			.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<bool>>>(), It.IsAny<CancellationToken>()))
			.Returns<Func<Task<bool>>, CancellationToken>(async (action, _) =>
			{
				calls.Add("domain-events:start");
				var result = await action();
				afterAction?.Invoke();
				calls.Add("domain-events:end");
				return result;
			});
		executor
			.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<CancellationToken>()))
			.Returns<Func<Task<int>>, CancellationToken>(async (action, _) =>
			{
				calls.Add("domain-events:start");
				var result = await action();
				afterAction?.Invoke();
				calls.Add("domain-events:end");
				return result;
			});
		return executor;
	}

	private static Mock<IPublishEndpoint> CreatePublisher(List<string> calls)
	{
		var publisher = new Mock<IPublishEndpoint>();
		publisher
			.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Callback(() => calls.Add("integration-event:publish"))
			.Returns(Task.CompletedTask);
		return publisher;
	}
}
