using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Application.Common.LRT;
using Application.Common.Models;
using Application.Common.NamedObject;
using Domain.CommonEntities;
using Microsoft.Extensions.Options;

namespace Api.Common.HostedServices;

public class LrtExecutorHostedService(
    IServiceScopeFactory scopeFactory,
    ILrtQuotaManager lrtQuotaManager,
    ILogger<LrtExecutorHostedService> logger,
    IOptionsMonitor<LrtExecutorOptions> options
) : BackgroundService
{
    private static readonly TimeSpan InitialLeaseDuration = TimeSpan.FromMinutes(5);
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

            while (lrtQuotaManager.TryUseQuota(Guid.NewGuid(), out var quota))
            {
                Job? job;

                try
                {
                    job = await leaseService.TryAcquireJobAsync(
                        quota.HolderId,
                        InitialLeaseDuration,
                        ct);
                }
                catch
                {
                    quota.Dispose();
                    throw;
                }

                if (job == null)
                {
                    quota.Dispose();
                    break;
                }
                
                _ = RunJobAndReleaseQuota(job.Id, job.SystemName, quota, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { logger.LogError(ex, "Job execution failed."); }
        finally { await Task.Delay(opt.Delay, ct); }
    }

    private async Task RunJobAndReleaseQuota(
        Guid jobId,
        string systemName,
        ILrtQuota lrtQuota,
        CancellationToken ct)
    {
        using (lrtQuota)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var registry = scope.ServiceProvider
                    .GetRequiredService<INamedObjectRegistry<LrtNamedObjectBase>>();

                await registry
                    .GetBySystemName(systemName)
                    .ExecuteAsync(
                        jobId,
                        lrtQuota.HolderId,
                        ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                //ignore
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled LRT runner error. JobId: {JobId}, SystemName: {SystemName}, HolderId: {HolderId}",
                    jobId,
                    systemName,
                    lrtQuota.HolderId);
            }
        }
    }
}