using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using MassTransit;

namespace Application.Common.Interfaces.Persistence;

public interface IApplicationTransactionContext : IDisposable, IAsyncDisposable
{
	IUnitOfWork UnitOfWork { get; }

	IRepositoryProvider Repositories { get; }

	IDomainEventExecutor DomainEventExecutor { get; }
	IIntegrationEventScope IntegrationEventScope { get; }
	IPublishEndpoint Publisher { get; }
	ITransactionExceptionClassifier ExceptionClassifier { get; }

	T GetRequiredService<T>() where T : notnull;
}
