using Application.Common.Interfaces.Lrt;
using Application.Common.Models;
using Microsoft.Extensions.Options;

namespace Api.Common.HostedServices;

public class ExpiredJobsWiperHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredJobsWiperHostedService> logger,
    IOptionsMonitor<LrtExecutorOptions> options) : BackgroundService
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
        LrtExecutorOptions opt,
        CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var leaseService = scope.ServiceProvider.GetRequiredService<IJobLeaseService>();
            
            var jobs = await leaseService.FailExpiredJobsWithoutAttempts(
                opt.MaxExpiredLeaseFailBatchSize,
                ct);

            if (jobs.Count > 0)
                logger.LogInformation(
                    "Jobs with expired leases are made 'Failed'. Count: {Count}",
                    jobs.Count);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { logger.LogError(ex, "Expired job wipe failed."); }
        finally { await Task.Delay(opt.Delay, ct); }
    }
}