using Application.Common.Interfaces.Events;

namespace Tests.Stubs;

public sealed class DomainEventExecutorStub : IDomainEventExecutor
{
	public Task DispatchBeforeSaveAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

	public Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default) => action();

	public Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default) =>
		action();
}
