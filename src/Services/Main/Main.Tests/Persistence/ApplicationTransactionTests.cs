using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Persistence;
using Attributes;
using Enums;
using FluentAssertions;
using Main.Entities.Product;
using Main.Entities.Producer;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Persistence.Services.UnitOfWork;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public sealed class ApplicationTransactionTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Factory_EachBatchStartsWithAnEmptyTrackerAndDisposesItsContext()
	{
		var factory = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>();
		var contexts = new List<DContext>();
		var initialTracked = Context.ChangeTracker.Entries().Count();
		for (var batch = 0; batch < 10; batch++)
		{
			await using var transaction = factory.Create();
			await transaction.ExecuteAsync((context, _) =>
			{
				context.GetRequiredService<IApplicationTransaction>().Should().BeSameAs(transaction);
				var db = context.GetRequiredService<DContext>();
				db.Should().NotBeSameAs(Context);
				db.ChangeTracker.Entries().Should().BeEmpty();
				contexts.Add(db);
				for (var item = 0; item < 100; item++)
					db.Producers.Add(Producer.Create($"batch-{batch}-{item}"));
				db.ChangeTracker.Entries().Should().HaveCount(100);
				return Task.CompletedTask;
			}, CancellationToken);
		}

		contexts.Distinct().Should().HaveCount(10);
		Context.ChangeTracker.Entries().Should().HaveCount(initialTracked);
		foreach (var db in contexts)
		{
			var accessDisposedContext = () => db.Set<Producer>();
			accessDisposedContext.Should().Throw<ObjectDisposedException>();
		}
	}

	[Fact]
	public void Classifier_ConcreteTypeAndInterfaceResolveToTheSameSingleton()
	{
		var classifier = Scope.ServiceProvider.GetRequiredService<ITransactionExceptionClassifier>();
		var concrete = Scope.ServiceProvider.GetRequiredService(classifier.GetType());
		using var otherScope = Sp.CreateScope();

		concrete.Should().BeSameAs(classifier);
		otherScope.ServiceProvider.GetRequiredService<ITransactionExceptionClassifier>().Should().BeSameAs(classifier);
		otherScope.ServiceProvider.GetRequiredService(classifier.GetType()).Should().BeSameAs(classifier);
	}

	[Fact]
	public async Task UnitOfWork_RetriesUsingTheInjectedClassifier()
	{
		var failure = new InvalidOperationException("custom classified failure");
		var classifier = new Mock<ITransactionExceptionClassifier>();
		classifier.Setup(value => value.Classify(failure))
			.Returns(new TransactionError(failure, TransactionErrorKind.ConcurrencyConflict));
		var unitOfWork = new EfUnitOfWorkBase(Context, classifier.Object);
		var attempts = 0;

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteWithTransaction<int>(
			new TransactionalAttribute(retryCount: 2, retryErrors: TransactionErrorKind.ConcurrencyConflict),
			() =>
			{
				attempts++;
				return Task.FromException<int>(failure);
			}, CancellationToken));

		exception.Should().BeSameAs(failure);
		attempts.Should().Be(3);
		classifier.Verify(value => value.Classify(failure), Times.Exactly(2));
		Context.Database.CurrentTransaction.Should().BeNull();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task ExhaustedRetries_RethrowTheOriginalErrorAndHandleItOnceAfterRollback(bool returnsResult)
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var failure = new PostgresException("retry", "ERROR", "ERROR", PostgresErrorCodes.SerializationFailure);
		var attempts = 0;
		var handlers = 0;
		var transactionIds = new List<Guid>();
		var transaction = service.WithSettings(TransactionalAttribute.RetryOnConflict(0, 2)).OnException(error =>
		{
			Context.Database.CurrentTransaction.Should().BeNull();
			error.Exception.Should().BeSameAs(failure);
			error.Kind.Should().Be(TransactionErrorKind.ConcurrencyConflict);
			handlers++;
		});

		async Task<int> Fail(CancellationToken cancellationToken)
		{
			attempts++;
			transactionIds.Add(Context.Database.CurrentTransaction!.TransactionId);
			await Context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
			throw failure;
		}

		Task Execute() => returnsResult
			? transaction.ExecuteAsync((_, ct) => Fail(ct), CancellationToken)
			: transaction.ExecuteAsync(async (_, ct) => { await Fail(ct); }, CancellationToken);
		var exception = await Assert.ThrowsAsync<PostgresException>(Execute);

		exception.Should().BeSameAs(failure);
		attempts.Should().Be(3);
		handlers.Should().Be(1);
		transactionIds.Distinct().Should().HaveCount(3);
		Context.Database.CurrentTransaction.Should().BeNull();
		await Context.Database.ExecuteSqlRawAsync("SELECT 1", CancellationToken);
	}

	[Fact]
	public async Task SuccessfulRetry_ReturnsTheValueWithoutCallingTheErrorHandler()
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var attempts = 0;
		var handled = false;

		var value = await service.WithSettings(TransactionalAttribute.RetryOnConflict(0, 2))
			.OnException(_ => handled = true)
			.ExecuteAsync((_, _) =>
			{
				if (++attempts < 3)
					throw new PostgresException("retry", "ERROR", "ERROR", PostgresErrorCodes.SerializationFailure);
				return Task.FromResult(42);
			}, CancellationToken);

		value.Should().Be(42);
		attempts.Should().Be(3);
		handled.Should().BeFalse();
		Context.Database.CurrentTransaction.Should().BeNull();
	}

	[Fact]
	public async Task NestedFailure_IsHandledByTheOwnerAfterTheDatabaseTransactionIsDisposed()
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var innerHandlers = 0;
		var outerHandlers = 0;
		var settings = TransactionalAttribute.ReadCommitted(0, 2);
		var outer = service.WithSettings(settings).OnException(error =>
		{
			Context.Database.CurrentTransaction.Should().BeNull();
			error.Kind.Should().Be(TransactionErrorKind.Unknown);
			outerHandlers++;
		});

		await Assert.ThrowsAsync<PostgresException>(() => outer.ExecuteAsync((_, _) =>
			service.OnException(_ => innerHandlers++)
				.ExecuteAsync(async (_, ct) =>
				{
					await Context.Database.ExecuteSqlRawAsync("SELECT 1 / 0", ct);
					return 1;
				}, CancellationToken), CancellationToken));

		outerHandlers.Should().Be(1);
		innerHandlers.Should().Be(1);
		await Context.Database.ExecuteSqlRawAsync("SELECT 1", CancellationToken);
	}

	[Theory]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation, PostgresErrorCodes.UniqueViolation, 3, 2)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation, PostgresErrorCodes.SerializationFailure, 3, 2)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation, PostgresErrorCodes.ForeignKeyViolation, 1, 2)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation, PostgresErrorCodes.DeadlockDetected, 3, 2)]
	[InlineData(TransactionErrorKind.None, PostgresErrorCodes.SerializationFailure, 3, 2)]
	[InlineData(TransactionErrorKind.None, PostgresErrorCodes.DeadlockDetected, 3, 2)]
	[InlineData(TransactionErrorKind.None, PostgresErrorCodes.LockNotAvailable, 3, 2)]
	[InlineData(TransactionErrorKind.None, PostgresErrorCodes.UniqueViolation, 1, 2)]
	[InlineData(TransactionErrorKind.None, PostgresErrorCodes.SerializationFailure, 1, 0)]
	public async Task RetryErrors_IncludeDefaultsAndAdditionalProviderErrors(
		TransactionErrorKind retryErrors, string sqlState, int expectedAttempts, int retryCount)
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var settings = new TransactionalAttribute(retryCount: retryCount, retryErrors: retryErrors);
		var failure = new PostgresException("failed", "ERROR", "ERROR", sqlState);
		var attempts = 0;
		var handled = 0;

		var exception = await Assert.ThrowsAsync<PostgresException>(() => service.WithSettings(settings)
			.OnException(_ =>
			{
				Context.Database.CurrentTransaction.Should().BeNull();
				handled++;
			})
			.ExecuteAsync<int>(async (_, ct) =>
			{
				attempts++;
				await Context.Database.ExecuteSqlRawAsync("SELECT 1", ct);
				throw failure;
			}, CancellationToken));

		exception.Should().BeSameAs(failure);
		attempts.Should().Be(expectedAttempts);
		handled.Should().Be(1);
	}

	[Theory]
	[InlineData(TransactionErrorKind.None, 2, 3)]
	[InlineData(TransactionErrorKind.UniqueConstraintViolation, 2, 3)]
	[InlineData(TransactionErrorKind.None, 0, 1)]
	public async Task RetryErrors_AlwaysIncludeEfConcurrencyUnlessRetriesAreDisabled(
		TransactionErrorKind retryErrors, int retryCount, int expectedAttempts)
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var settings = new TransactionalAttribute(retryCount: retryCount, retryErrors: retryErrors);
		var failure = new DbUpdateConcurrencyException("conflict");
		var attempts = 0;

		var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => service.WithSettings(settings)
			.ExecuteAsync<int>((_, _) =>
			{
				attempts++;
				return Task.FromException<int>(failure);
			}, CancellationToken));

		exception.Should().BeSameAs(failure);
		attempts.Should().Be(expectedAttempts);
		Context.Database.CurrentTransaction.Should().BeNull();
	}

	[Fact]
	public async Task Cancellation_RollsBackDatabaseWritesWithoutInvokingTheErrorHandler()
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		using var cancellation = new CancellationTokenSource();
		var handled = false;
		var transaction = service.WithSettings(TransactionalAttribute.ReadCommitted(0, 2)).OnException(_ => handled = true);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => transaction.ExecuteAsync(async (_, ct) =>
		{
			Context.Characteristics.Add(Characteristic.Create("Cancelled"));
			await Context.SaveChangesAsync(ct);
			await cancellation.CancelAsync();
			ct.ThrowIfCancellationRequested();
		}, cancellation.Token));

		handled.Should().BeFalse();
		Context.Database.CurrentTransaction.Should().BeNull();
		(await Context.Characteristics.AsNoTracking().AnyAsync(characteristic => characteristic.Name == "Cancelled", CancellationToken))
			.Should().BeFalse();
	}

	[Theory]
	[InlineData(PostgresErrorCodes.ForeignKeyViolation, TransactionErrorKind.ForeignKeyViolation)]
	[InlineData(PostgresErrorCodes.UniqueViolation, TransactionErrorKind.UniqueConstraintViolation)]
	[InlineData(PostgresErrorCodes.NotNullViolation, TransactionErrorKind.NotNullViolation)]
	[InlineData(PostgresErrorCodes.CheckViolation, TransactionErrorKind.CheckConstraintViolation)]
	[InlineData(PostgresErrorCodes.SerializationFailure, TransactionErrorKind.ConcurrencyConflict)]
	[InlineData(PostgresErrorCodes.DeadlockDetected, TransactionErrorKind.Deadlock)]
	[InlineData(PostgresErrorCodes.LockNotAvailable, TransactionErrorKind.LockUnavailable)]
	[InlineData(PostgresErrorCodes.DivisionByZero, TransactionErrorKind.Unknown)]
	[InlineData("ZZ000", TransactionErrorKind.Unknown)]
	public void Classifier_FindsWrappedProviderErrorsAndKeepsTheOriginalException(string sqlState, TransactionErrorKind expected)
	{
		var provider = new PostgresException("failed", "ERROR", "ERROR", sqlState, constraintName: "test_constraint");
		Exception failure = provider;
		for (var depth = 0; depth < 8; depth++)
			failure = new Exception("wrapper", failure);
		var classifier = Scope.ServiceProvider.GetRequiredService<ITransactionExceptionClassifier>();

		var error = classifier.Classify(failure);

		error.Exception.Should().BeSameAs(failure);
		error.Kind.Should().Be(expected);
		error.ConstraintName.Should().Be("test_constraint");
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Classifier_MapsEfConcurrencyConflicts(bool wrapped)
	{
		Exception failure = new DbUpdateConcurrencyException("conflict");
		if (wrapped) failure = new InvalidOperationException("wrapper", failure);
		var classifier = Scope.ServiceProvider.GetRequiredService<ITransactionExceptionClassifier>();

		var error = classifier.Classify(failure);

		error.Exception.Should().BeSameAs(failure);
		error.Kind.Should().Be(TransactionErrorKind.ConcurrencyConflict);
		error.ConstraintName.Should().BeNull();
	}

	[Fact]
	public void Classifier_MapsUnrecognizedExceptionsToUnknown()
	{
		var failure = new InvalidOperationException("unknown");
		var classifier = Scope.ServiceProvider.GetRequiredService<ITransactionExceptionClassifier>();

		var error = classifier.Classify(failure);

		error.Exception.Should().BeSameAs(failure);
		error.Kind.Should().Be(TransactionErrorKind.Unknown);
		error.ConstraintName.Should().BeNull();
	}
}
