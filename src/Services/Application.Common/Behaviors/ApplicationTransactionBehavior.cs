using System.Reflection;
using Application.Common.Interfaces.Persistence;
using Attributes;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class ApplicationTransactionBehavior<TRequest, TResponse>(
    IApplicationTransactionService transactionService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly TransactionalAttribute? Settings =
        typeof(TRequest).GetCustomAttribute<TransactionalAttribute>(true);
    private static readonly AutoSaveAttribute? AutoSave =
        typeof(TRequest).GetCustomAttribute<AutoSaveAttribute>(true);

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return transactionService.ExecuteAsync(
            Settings,
            async (context, ct) =>
            {
                var response = await next(ct);

                if (AutoSave is not null &&
                    !context.UnitOfWork.Context.SuppressAutoSave)
                    await context.UnitOfWork.SaveChangesAsync(ct);

                return response;
            },
            cancellationToken);
    }
}
