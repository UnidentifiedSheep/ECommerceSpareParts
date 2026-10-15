using Application.Common.Interfaces.Persistence;
using Attributes;
using Enums;
using FluentAssertions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public sealed class ApplicationTransactionTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task ExhaustedRetries_RethrowTheOriginalErrorAndHandleItOnceAfterRollback(bool returnsResult)
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionService>();
		var failure = new PostgresException("retry", "ERROR", "ERROR", PostgresErrorCodes.SerializationFailure);
		var attempts = 0;
		var handlers = 0;
		var transactionIds = new List<Guid>();
		var transaction = service.Create(TransactionalAttribute.RetryOnConflict(0, 2)).OnException(error =>
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
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionService>();
		var attempts = 0;
		var handled = false;

		var value = await service.Create(TransactionalAttribute.RetryOnConflict(0, 2))
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
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionService>();
		var innerHandlers = 0;
		var outerHandlers = 0;
		var settings = TransactionalAttribute.ReadCommitted(0, 2);
		var outer = service.Create(settings).OnException(error =>
		{
			Context.Database.CurrentTransaction.Should().BeNull();
			error.Kind.Should().Be(TransactionErrorKind.Unknown);
			outerHandlers++;
		});

		await Assert.ThrowsAsync<PostgresException>(() => outer.ExecuteAsync((_, _) =>
			service.Create(settings).OnException(_ => innerHandlers++)
				.ExecuteAsync(async (_, ct) =>
				{
					await Context.Database.ExecuteSqlRawAsync("SELECT 1 / 0", ct);
					return 1;
				}, CancellationToken), CancellationToken));

		outerHandlers.Should().Be(1);
		innerHandlers.Should().Be(0);
		await Context.Database.ExecuteSqlRawAsync("SELECT 1", CancellationToken);
	}

	[Fact]
	public async Task Cancellation_RollsBackDatabaseWritesWithoutInvokingTheErrorHandler()
	{
		var service = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionService>();
		using var cancellation = new CancellationTokenSource();
		var handled = false;
		var transaction = service.Create(TransactionalAttribute.ReadCommitted(0, 2)).OnException(_ => handled = true);

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
