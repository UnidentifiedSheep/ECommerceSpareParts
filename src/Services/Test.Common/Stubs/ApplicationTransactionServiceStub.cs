using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;

namespace Tests.Stubs;

public sealed class ApplicationTransactionServiceStub(
	IUnitOfWork unitOfWork,
	IRepositoryProvider repositories) : IApplicationTransactionService
{
	private readonly IApplicationTransactionContext _context =
		new TestApplicationTransactionContext(unitOfWork, repositories);

	public Task ExecuteAsync(
		TransactionalAttribute? settings,
		Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default) => action(_context, cancellationToken);

	public Task<TResult> ExecuteAsync<TResult>(
		TransactionalAttribute? settings,
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default) => action(_context, cancellationToken);

	private sealed class TestApplicationTransactionContext(
		IUnitOfWork unitOfWork,
		IRepositoryProvider repositories) : IApplicationTransactionContext
	{
		public IUnitOfWork UnitOfWork { get; } = unitOfWork;

		public IRepositoryProvider Repositories { get; } = repositories;
	}
}
