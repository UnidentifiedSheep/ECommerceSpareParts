using System.Diagnostics;
using System.Reflection;
using Attributes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public class DiagnosticsBehavior<TRequest, TResponse>(
    ILogger<DiagnosticsBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private static readonly DiagnosticsAttribute? Settings =
        typeof(TRequest).GetCustomAttribute<DiagnosticsAttribute>(true);
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (Settings is not { Enabled: true }) 
            return await next(cancellationToken);
        
        var timer = new Stopwatch();
        timer.Start();
        
        var response = await next(cancellationToken);
        
        timer.Stop();
        var timeTaken = timer.Elapsed;
        
        if (timeTaken.Milliseconds > Settings.MaxExecutionTimeMs)
            logger.LogWarning("[PERFORMANCE] The request {Request} took {TimeTaken} seconds." +
                              "Maximum expected execution time is {Expected}",
                typeof(TRequest).Name, 
                timeTaken.Seconds,
                TimeSpan.FromMilliseconds(Settings.MaxExecutionTimeMs));
        
        
        logger.LogInformation(
            "[END] Handled {Request}. Time taken is {TimeTaken}", 
            typeof(TRequest).Name, 
            timeTaken);
        
        return response;
    }
}