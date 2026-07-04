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
            await Iteration(currentValue, stoppingToken);
        }
    }

    private async Task Iteration(
        ScheduledJobEnqueuerOptions opt,
        CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send(
                new QueueScheduledJobsCommand(opt.BatchSize),
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { logger.LogError(ex, "Scheduled job enqueuer failed."); }
        finally { await Task.Delay(opt.Delay, ct); }
    }
}