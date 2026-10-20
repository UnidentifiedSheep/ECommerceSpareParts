using Abstractions.Models;
using Application.Common.Behaviors;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Enums;
using FluentAssertions;
using MediatR;
using Moq;
using Persistence;
using Tests.Stubs;

namespace Tests.Tests.Behaviors;

public sealed class ApplicationTransactionBehaviorTests
{
	[Fact]
	public async Task Handle_UnannotatedRequest_DelegatesWithoutAnOuterBoundary()
	{
		var service = new RecordingApplicationTransaction();
		var behavior = new ApplicationTransactionBehavior<TestRequest, Unit>(service);

		var result = await behavior.Handle(
			new TestRequest(),
			_ => Task.FromResult(Unit.Value),
			CancellationToken.None);

		result.Should().Be(Unit.Value);
		service.Calls.Should().Be(0);
		service.Settings.Should().BeNull();
	}

	[Fact]
	public async Task Handle_TransactionalRequest_PassesSettingsToService()
	{
		var service = new RecordingApplicationTransaction();
		var behavior = new ApplicationTransactionBehavior<TransactionalRequest, Unit>(service);

		await behavior.Handle(
			new TransactionalRequest(),
			_ => Task.FromResult(Unit.Value),
			CancellationToken.None);

		service.Calls.Should().Be(1);
		service.Settings.Should().NotBeNull();
		service.Settings!.RetryErrors.Should().Be(TransactionalAttribute.DefaultRetryErrors);
	}

	[Fact]
	public async Task Handle_TransactionalRequest_PreservesTheExplicitRetryMask()
	{
		var service = new RecordingApplicationTransaction();
		var behavior = new ApplicationTransactionBehavior<CustomRetryRequest, Unit>(service);

		await behavior.Handle(
			new CustomRetryRequest(),
			_ => Task.FromResult(Unit.Value),
			CancellationToken.None);

		service.Settings!.RetryErrors.Should().Be(
			TransactionErrorKind.UniqueConstraintViolation | TransactionErrorKind.ConcurrencyConflict);
		service.Settings.RetryCount.Should().Be(2);
	}

	[Fact]
	public async Task Handle_AutoSaveRequest_SavesBeforeBoundaryCompletes()
	{
		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork.SetupGet(x => x.Context).Returns(new UnitOfWorkContext());
		unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		var context = new Mock<IApplicationTransactionContext>();
		context.SetupGet(x => x.UnitOfWork).Returns(unitOfWork.Object);
		var service = new RecordingApplicationTransaction(context.Object);
		var behavior = new ApplicationTransactionBehavior<AutoSaveRequest, Unit>(service);

		await behavior.Handle(
			new AutoSaveRequest(),
			_ => Task.FromResult(Unit.Value),
			CancellationToken.None);

		unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

    [Fact]
    public async Task Handle_UnannotatedRequest_LetsTheServiceConfigureItsTransaction()
    {
        var transaction = new RecordingApplicationTransaction();
        var settings = TransactionalAttribute.ReadCommitted(20, 3);
        var behavior = new ApplicationTransactionBehavior<TestRequest, Unit>(transaction);

        await behavior.Handle(new TestRequest(), ct => transaction.WithSettings(settings)
            .ExecuteAsync((_, _) => Task.FromResult(Unit.Value), ct), TestContext.Current.CancellationToken);

        transaction.Calls.Should().Be(1);
        transaction.Settings.Should().BeSameAs(settings);
    }

	private sealed record TestRequest : IRequest<Unit>;

	[Transactional]
	private sealed record TransactionalRequest : IRequest<Unit>;

	[Transactional(retryCount: 2, retryErrors: TransactionErrorKind.UniqueConstraintViolation | TransactionErrorKind.ConcurrencyConflict)]
	private sealed record CustomRetryRequest : IRequest<Unit>;

	[AutoSave]
	private sealed record AutoSaveRequest : IRequest<Unit>;

	private sealed class RecordingApplicationTransaction(
		IApplicationTransactionContext? context = null) : ApplicationTransactionStub(context!);
}
