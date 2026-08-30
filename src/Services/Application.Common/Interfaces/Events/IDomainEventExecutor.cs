namespace Application.Common.Interfaces.Events;

public interface IDomainEventExecutor
{
	Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);

	Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}
