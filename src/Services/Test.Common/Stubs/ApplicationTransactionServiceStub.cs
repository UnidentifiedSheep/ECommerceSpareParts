using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;

namespace Tests.Stubs;

public sealed class ApplicationTransactionServiceStub(
	IUnitOfWork unitOfWork,
	IRepositoryProvider repositories,
	IIntegrationEventScope integrationEventScope) : IApplicationTransactionService
{
	private readonly IApplicationTransactionContext _context =
		new TestApplicationTransactionContext(unitOfWork, repositories, integrationEventScope);

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
		IRepositoryProvider repositories,
		IIntegrationEventScope integrationEventScope) : IApplicationTransactionContext
	{
		public IUnitOfWork UnitOfWork => unitOfWork;

		public IRepositoryProvider Repositories => repositories;
		public IIntegrationEventScope IntegrationEventScope => integrationEventScope;
	}
}
