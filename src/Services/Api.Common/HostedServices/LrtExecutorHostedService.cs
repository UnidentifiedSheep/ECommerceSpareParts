using Api.Common.Models.Options;
using Application.Common.Handlers.Jobs;
using MediatR;
using Microsoft.Extensions.Options;

namespace Api.Common.HostedServices;

public class LrtExecutorHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<LrtExecutorHostedService> logger,
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
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send(
                new RunJobBatchCommand(opt.MaxParallelPerWorker),
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job batch execution failed.");
        }
        finally
        {
            await Task.Delay(opt.Delay, ct);
        }
        
    }
}