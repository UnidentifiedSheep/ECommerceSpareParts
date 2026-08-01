using Api.Common.Models.Options;
using Application.Common.Handlers.JobSchedules;
using Application.Common.Models;
using Application.Common.Models.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace Api.Common.HostedServices;

public class ScheduledJobEnqueuerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ScheduledJobEnqueuerHostedService> logger,
    IOptionsMonitor<ScheduledJobEnqueuerOptions> options
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentValue = options.CurrentValue;
            var batchIsFull = await Iteration(currentValue, stoppingToken);

            if (!batchIsFull)
                await Task.Delay(currentValue.Delay, stoppingToken);
        }
    }

    private async Task<bool> Iteration(
        ScheduledJobEnqueuerOptions opt,
        CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await sender.Send(
                new QueueScheduledJobsCommand(opt.BatchSize),
                ct);

            return result.Queued == opt.BatchSize;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scheduled job enqueuer failed.");
            return false;
        }
    }
}
