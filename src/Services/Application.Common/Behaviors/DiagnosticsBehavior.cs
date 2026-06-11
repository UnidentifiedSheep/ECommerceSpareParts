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
        var elapsedMs = timer.Elapsed.TotalMilliseconds;

        if (elapsedMs > Settings.MaxExecutionTimeMs)
            logger.LogWarning(
                "[PERFORMANCE] {Request} took {ElapsedMs:F0} ms. Expected <= {ExpectedMs} ms",
                typeof(TRequest).Name,
                elapsedMs,
                Settings.MaxExecutionTimeMs);
        

        logger.LogInformation(
            "[END] {Request} handled in {ElapsedMs:F0} ms",
            typeof(TRequest).Name,
            elapsedMs);
        
        return response;
    }
}