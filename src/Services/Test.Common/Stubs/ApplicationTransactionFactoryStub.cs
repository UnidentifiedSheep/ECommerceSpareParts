using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using MassTransit;

namespace Tests.Stubs;

public sealed class ApplicationTransactionFactoryStub(
	IUnitOfWork unitOfWork,
	IRepositoryProvider repositories,
	IIntegrationEventScope integrationEventScope,
	IServiceProvider? services = null) : IApplicationTransactionFactory
{
	public IApplicationTransaction Create() => new ApplicationTransactionStub(
		new TestApplicationTransactionContext(unitOfWork, repositories, integrationEventScope, services));

	private sealed class TestApplicationTransactionContext(
		IUnitOfWork unitOfWork,
		IRepositoryProvider repositories,
		IIntegrationEventScope integrationEventScope,
		IServiceProvider? services) : IApplicationTransactionContext
	{
		public IUnitOfWork UnitOfWork => unitOfWork;
		public IRepositoryProvider Repositories => repositories;
		public IDomainEventExecutor DomainEventExecutor { get; } = new DomainEventExecutorStub();
		public IIntegrationEventScope IntegrationEventScope => integrationEventScope;
		public IPublishEndpoint Publisher { get; } = new MessageBrokerStub();
		public ITransactionExceptionClassifier ExceptionClassifier { get; } = Moq.Mock.Of<ITransactionExceptionClassifier>();

		public T GetRequiredService<T>() where T : notnull => typeof(T) switch
		{
			var type when type == typeof(IUnitOfWork) => (T)(object)UnitOfWork,
			var type when type == typeof(IRepositoryProvider) => (T)(object)Repositories,
			var type when type == typeof(IIntegrationEventScope) => (T)(object)IntegrationEventScope,
			var type when type == typeof(IPublishEndpoint) => (T)(object)Publisher,
			var type when type == typeof(IDomainEventExecutor) => (T)(object)DomainEventExecutor,
			var type when type == typeof(ITransactionExceptionClassifier) => (T)(object)ExceptionClassifier,
			_ => services?.GetService(typeof(T)) is T service ? service :
				throw new InvalidOperationException($"Test service {typeof(T)} is not registered.")
		};

		public void Dispose() { }

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}
}
