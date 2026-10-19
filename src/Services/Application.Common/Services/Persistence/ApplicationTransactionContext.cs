using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Services.Persistence;

public class ApplicationTransactionContext(IServiceProvider services) : IApplicationTransactionContext
{
	public IUnitOfWork UnitOfWork => GetRequiredService<IUnitOfWork>();
	public IRepositoryProvider Repositories => GetRequiredService<IRepositoryProvider>();
	public IDomainEventExecutor DomainEventExecutor => GetRequiredService<IDomainEventExecutor>();
	public IIntegrationEventScope IntegrationEventScope => GetRequiredService<IIntegrationEventScope>();
	public IPublishEndpoint Publisher => GetRequiredService<IPublishEndpoint>();
	public ITransactionExceptionClassifier ExceptionClassifier => GetRequiredService<ITransactionExceptionClassifier>();

	public virtual T GetRequiredService<T>() where T : notnull => services.GetRequiredService<T>();

	public virtual void Dispose() => GC.SuppressFinalize(this);

	public virtual ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}
}
