using Abstractions.Interfaces.Persistence;
using Abstractions.Models;
using Application.Common.Behaviors;
using Application.Common.Interfaces.Persistence;
using Attributes;
using FluentAssertions;
using MediatR;
using Moq;

namespace Tests.Tests.Behaviors;

public sealed class ApplicationTransactionBehaviorTests
{
	[Fact]
	public async Task Handle_RequestWithoutTransaction_DelegatesWithoutSettings()
	{
		var service = new RecordingApplicationTransactionService();
		var behavior = new ApplicationTransactionBehavior<TestRequest, Unit>(service);

		var result = await behavior.Handle(
			new TestRequest(),
			_ => Task.FromResult(Unit.Value),
			CancellationToken.None);

		result.Should().Be(Unit.Value);
		service.Calls.Should().Be(1);
		service.Settings.Should().BeNull();
	}

	[Fact]
	public async Task Handle_TransactionalRequest_PassesSettingsToService()
	{
		var service = new RecordingApplicationTransactionService();
		var behavior = new ApplicationTransactionBehavior<TransactionalRequest, Unit>(service);

		await behavior.Handle(
			new TransactionalRequest(),
			_ => Task.FromResult(Unit.Value),
			CancellationToken.None);

		service.Calls.Should().Be(1);
		service.Settings.Should().NotBeNull();
	}

	[Fact]
	public async Task Handle_AutoSaveRequest_SavesBeforeBoundaryCompletes()
	{
		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork.SetupGet(x => x.Context).Returns(new UnitOfWorkContext());
		unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		var context = new Mock<IApplicationTransactionContext>();
		context.SetupGet(x => x.UnitOfWork).Returns(unitOfWork.Object);
		var service = new RecordingApplicationTransactionService(context.Object);
		var behavior = new ApplicationTransactionBehavior<AutoSaveRequest, Unit>(service);

		await behavior.Handle(
			new AutoSaveRequest(),
			_ => Task.FromResult(Unit.Value),
			CancellationToken.None);

		unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	private sealed record TestRequest : IRequest<Unit>;

	[Transactional]
	private sealed record TransactionalRequest : IRequest<Unit>;

	[AutoSave]
	private sealed record AutoSaveRequest : IRequest<Unit>;

	private sealed class RecordingApplicationTransactionService(
		IApplicationTransactionContext? context = null) : IApplicationTransactionService
	{
		public int Calls { get; private set; }

		public TransactionalAttribute? Settings { get; private set; }

		public async Task ExecuteAsync(
			TransactionalAttribute? settings,
			Func<IApplicationTransactionContext, CancellationToken, Task> action,
			CancellationToken cancellationToken = default)
		{
			Calls++;
			Settings = settings;
			await action(context!, cancellationToken);
		}

		public async Task<TResult> ExecuteAsync<TResult>(
			TransactionalAttribute? settings,
			Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
			CancellationToken cancellationToken = default)
		{
			Calls++;
			Settings = settings;
			return await action(context!, cancellationToken);
		}
	}
}
