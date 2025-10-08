using System.Reflection;
using Core.Attributes;
using Core.Interfaces.Services;
using MediatR;

namespace Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly TransactionalAttribute? Settings =
        typeof(TRequest).GetCustomAttribute<TransactionalAttribute>(true);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (Settings is null)
            return await next(cancellationToken);

        return await unitOfWork.ExecuteWithTransaction(Settings, async () =>
        {
            var response = await next(cancellationToken);
            return response;
        }, cancellationToken);
    }
}