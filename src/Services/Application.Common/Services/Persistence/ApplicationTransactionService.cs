using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using MassTransit;

namespace Application.Common.Services.Persistence;

public sealed class ApplicationTransactionService(
    IUnitOfWork unitOfWork,
    IDomainEventExecutor domainEventExecutor,
    IIntegrationEventScope integrationEventScope,
    IPublishEndpoint publisher,
    IApplicationTransactionContext context) : IApplicationTransactionService
{ //TODO it should work also in services with out db.
    public Task ExecuteAsync(
        TransactionalAttribute? settings,
        Func<IApplicationTransactionContext, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        return ExecuteAsync(
            settings,
            async (transactionContext, ct) =>
            {
                await action(transactionContext, ct);
                return true;
            },
            cancellationToken);
    }

    public Task<TResult> ExecuteAsync<TResult>(
        TransactionalAttribute? settings,
        Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        Task<TResult> ExecuteCoreAsync()
        {
            return ExecuteBoundaryAsync(action, cancellationToken);
        }

        return settings is null
            ? ExecuteCoreAsync()
            : unitOfWork.ExecuteWithTransaction(
                settings,
                ExecuteCoreAsync,
                cancellationToken);
    }

    private async Task<TResult> ExecuteBoundaryAsync<TResult>(
        Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await domainEventExecutor.ExecuteAsync(
                () => action(context, cancellationToken),
                cancellationToken);

            var integrationEvents = integrationEventScope.Flush();
            foreach (var @event in integrationEvents)
                if (@event.RoutingKey is null)
                    await publisher.Publish(
                        @event.Message,
                        cancellationToken);
                else
                    await publisher.Publish(
                        @event.Message,
                        pubContext => pubContext.SetRoutingKey(@event.RoutingKey),
                        cancellationToken);

            if (integrationEvents.Count != 0 &&
                !unitOfWork.Context.SuppressAutoSave)
                await unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch
        {
            integrationEventScope.Flush();
            throw;
        }
    }
}
