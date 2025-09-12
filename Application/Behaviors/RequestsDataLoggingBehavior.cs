using Application.Interfaces;
using Core.Interfaces;
using Core.Models;
using MediatR;

namespace Application.Behaviors;

public class RequestsDataLoggingBehavior<TRequest, TResponse>(
    IEnumerable<ILoggableRequest<TRequest>> logParams,
    ISearchLogger searchLogger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var logParam = logParams.FirstOrDefault();
        if (logParam == null)
            return await next(cancellationToken);

        var userId = logParam.GetUserId(request);
        if (!logParam.IsLoggingNeeded(request) || userId == null) return await next(cancellationToken);

        var searchModel = new SearchLogModel(userId, logParam.GetLogPlace(request), logParam.GetLogData(request));
        searchLogger.Enqueue(searchModel);

        return await next(cancellationToken);
    }
}