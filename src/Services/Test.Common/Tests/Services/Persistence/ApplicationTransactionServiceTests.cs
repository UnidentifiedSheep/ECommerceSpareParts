using Abstractions.Models;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Exceptions;
using Application.Common.Models.Persistence;
using Application.Common.Services.Events;
using Application.Common.Services.Persistence;
using Attributes;
using Enums;
using FluentAssertions;
using MassTransit;
using Moq;
using Persistence;

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
			context,
			Moq.Mock.Of<Application.Common.Interfaces.Persistence.ITransactionExceptionClassifier>());

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
			Mock.Of<IApplicationTransactionContext>(),
			Moq.Mock.Of<Application.Common.Interfaces.Persistence.ITransactionExceptionClassifier>());

		var result = await service.ExecuteAsync(
			null,
			(_, _) =>
			{
				calls.Add("action");
				return Task.FromResult(42);
			},
			TestContext.Current.CancellationToken);

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
			Mock.Of<IApplicationTransactionContext>(),
			Moq.Mock.Of<Application.Common.Interfaces.Persistence.ITransactionExceptionClassifier>());

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
			Mock.Of<IApplicationTransactionContext>(),
			Moq.Mock.Of<Application.Common.Interfaces.Persistence.ITransactionExceptionClassifier>());

		await service.ExecuteAsync(
			null,
			(_, _) =>
			{
				integrationEventScope.Add("routed", "test-service");
				return Task.CompletedTask;
			},
			TestContext.Current.CancellationToken);

		publisher.Verify(
			x => x.Publish(
				It.Is<object>(message => Equals(message, "routed")),
				It.IsAny<IPipe<PublishContext>>(),
				It.IsAny<CancellationToken>()),
			Times.Once);
		publisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Create_IsolatesHandlersAndRethrowsUnhandledVoidAndGenericFailures()
	{
		var calls = new List<string>();
		var service = CreateService(calls);
		var firstCalls = 0;
		var secondCalls = 0;
		var first = service.Create().OnException(_ => firstCalls++);
		var second = service.Create().OnException(_ => secondCalls++);
		var firstFailure = new InvalidOperationException("first");
		var secondFailure = new InvalidOperationException("second");

		var firstException = await Assert.ThrowsAsync<InvalidOperationException>(() => first.ExecuteAsync<int>(
			(_, _) => Task.FromException<int>(firstFailure), TestContext.Current.CancellationToken));
		firstException.Should().BeSameAs(firstFailure);
		firstCalls.Should().Be(1);
		secondCalls.Should().Be(0);

		var secondException = await Assert.ThrowsAsync<InvalidOperationException>(() => second.ExecuteAsync(
			(_, _) => Task.FromException(secondFailure), TestContext.Current.CancellationToken));
		secondException.Should().BeSameAs(secondFailure);
		firstCalls.Should().Be(1);
		secondCalls.Should().Be(1);
	}

	[Fact]
	public async Task OnException_CanReplaceTheOriginalFailure()
	{
		var service = CreateService([]);
		var original = new InvalidOperationException("original");
		var interrupted = new LrtInterruptedException("invalid import");
		var transaction = service.Create().OnException(error =>
		{
			error.Exception.Should().BeSameAs(original);
			error.Kind.Should().Be(TransactionErrorKind.Unknown);
			error.ConstraintName.Should().BeNull();
			throw interrupted;
		});

		var exception = await Assert.ThrowsAsync<LrtInterruptedException>(() => transaction.ExecuteAsync<int>(
			(_, _) => Task.FromException<int>(original), TestContext.Current.CancellationToken));

		exception.Should().BeSameAs(interrupted);
	}

	[Fact]
	public async Task OnException_DoesNotHandleCancellation()
	{
		var service = CreateService([]);
		var handled = false;
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();
		var transaction = service.Create().OnException(_ => handled = true);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => transaction.ExecuteAsync<int>(
			(_, _) => Task.FromCanceled<int>(cancellation.Token), cancellation.Token));

		handled.Should().BeFalse();
	}

	[Fact]
	public async Task OnException_DefersToTheOwnerOfAnEnclosingTransaction()
	{
		var calls = new List<string>();
		var unitOfWork = CreateUnitOfWork(calls);
		unitOfWork.SetupGet(work => work.HasActiveTransaction).Returns(true);
		var service = CreateService(calls, unitOfWork);
		var handled = false;
		var original = new InvalidOperationException("nested");
		var transaction = service.Create().OnException(_ => handled = true);

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => transaction.ExecuteAsync<int>(
			(_, _) => Task.FromException<int>(original), TestContext.Current.CancellationToken));

		exception.Should().BeSameAs(original);
		handled.Should().BeFalse();
	}

	[Theory]
	[InlineData(TransactionErrorKind.ForeignKeyViolation, TransactionErrorKind.ForeignKeyViolation, true)]
	[InlineData(TransactionErrorKind.ForeignKeyViolation, TransactionErrorKind.ConcurrencyConflict, false)]
	[InlineData(TransactionErrorKind.ForeignKeyViolation, TransactionErrorKind.Unknown, false)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation | TransactionErrorKind.ConcurrencyConflict, TransactionErrorKind.UniqueConstraintViolation, true)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation | TransactionErrorKind.ConcurrencyConflict, TransactionErrorKind.ConcurrencyConflict, true)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation | TransactionErrorKind.ConcurrencyConflict, TransactionErrorKind.ForeignKeyViolation, false)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation | TransactionErrorKind.ConcurrencyConflict, TransactionErrorKind.Unknown, false)]
	[InlineData(TransactionErrorKind.Unknown, TransactionErrorKind.Unknown, true)]
	[InlineData(TransactionErrorKind.Unknown | TransactionErrorKind.ConcurrencyConflict, TransactionErrorKind.Unknown, true)]
	[InlineData(TransactionErrorKind.None, TransactionErrorKind.ForeignKeyViolation, false)]
	[InlineData(TransactionErrorKind.None, TransactionErrorKind.Unknown, false)]
	[InlineData(TransactionErrorKind.None, TransactionErrorKind.None, false)]
	[InlineData(TransactionErrorKind.Unknown, TransactionErrorKind.None, false)]
	public async Task OnException_WithKind_OnlyInvokesTheMatchingHandler(TransactionErrorKind filter, TransactionErrorKind kind, bool expected)
	{
		var calls = new List<string>();
		var original = new InvalidOperationException("failed");
		var classifier = new Mock<ITransactionExceptionClassifier>();
		classifier.Setup(value => value.Classify(original)).Returns(new TransactionError(original, kind));
		var service = new ApplicationTransactionService(CreateUnitOfWork(calls).Object,
			CreateDomainEventExecutor(calls).Object, new IntegrationEventScope(), CreatePublisher(calls).Object,
			Mock.Of<IApplicationTransactionContext>(), classifier.Object);
		var handled = false;

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.Create()
			.OnException(filter, _ => handled = true)
			.ExecuteAsync<int>((_, _) => Task.FromException<int>(original), TestContext.Current.CancellationToken));

		handled.Should().Be(expected);
		exception.Should().BeSameAs(original);
	}

	private static ApplicationTransactionService CreateService(List<string> calls, Mock<IUnitOfWork>? unitOfWork = null)
	{
		var classifier = new Mock<ITransactionExceptionClassifier>();
		classifier.Setup(value => value.Classify(It.IsAny<Exception>()))
			.Returns((Exception exception) => new TransactionError(exception));
		return new ApplicationTransactionService(
			(unitOfWork ?? CreateUnitOfWork(calls)).Object,
			CreateDomainEventExecutor(calls).Object,
			new IntegrationEventScope(),
			CreatePublisher(calls).Object,
			Mock.Of<IApplicationTransactionContext>(),
			classifier.Object);
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
