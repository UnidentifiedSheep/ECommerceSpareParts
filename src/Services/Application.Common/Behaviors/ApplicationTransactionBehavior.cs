using Application.Common.Extensions;
using System.Reflection;
using Application.Common.Interfaces.Persistence;
using Attributes;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class ApplicationTransactionBehavior<TRequest, TResponse>(
	IApplicationTransaction transactionService) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse> where TResponse : notnull
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
        // Unannotated requests let their transactional services own the boundary.
        if (Settings is null && AutoSave is null)
            return next(cancellationToken);

		return transactionService.WithSettingsIfIdle(Settings).ExecuteAsync(
			async (context, ct) =>
			{
				var response = await next(ct);

				if (AutoSave is not null && !context.UnitOfWork.Context.SuppressAutoSave)
					await context.UnitOfWork.SaveChangesAsync(ct);

				return response;
			},
			cancellationToken);
	}
}
