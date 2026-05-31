using Analytics.Application.Handlers.Metrics.ScheduleDirtyMetricsRecalculation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Analytics.Worker.HostedServices;

public class RecalculationCheckHostedService(
    IOptionsMonitor<HostedServiceOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<RecalculationCheckHostedService> logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentValue = options.CurrentValue.RecalculationCheck;
            await Iteration(currentValue, stoppingToken);
        }
    }

    private async Task Iteration(RecalculationCheckOptions opt, CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send(
                new ScheduleDirtyMetricsRecalculationCommand(opt.ScheduleAtOnce), 
                ct);
            await Task.Delay(opt.Delay, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Recalculation check failed.");
        }
    }
}
