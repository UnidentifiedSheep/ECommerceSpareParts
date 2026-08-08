using System.Diagnostics;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Models;
using MediatR;

namespace Application.Common.Behaviors;

public class MetricsBehavior<TRequest, TResponse>(
    CqrsMetrics metrics) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var tags = new TagList
        {
            { "cqrs.request.name", requestName },
            { "cqrs.request.type", GetRequestType() }
        };

        metrics.Requests.Add(1, tags);

        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await next(cancellationToken);
            return response;
        }
        catch
        {
            metrics.Errors.Add(1, tags);
            throw;
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startedAt);
            metrics.Duration.Record(elapsed.TotalMilliseconds, tags);
        }
    }
    
    private static string GetRequestType()
    {
        if (typeof(TRequest).IsAssignableTo(typeof(ICommand<TResponse>)))
            return "command";

        if (typeof(TRequest).IsAssignableTo(typeof(IQuery<TResponse>)))
            return "query";

        return "unknown";
    }
}
