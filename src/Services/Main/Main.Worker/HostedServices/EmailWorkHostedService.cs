using Main.Application.Handlers.Mailing.SendMailBatch;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Worker.HostedServices;

public class EmailWorkHostedService(
    IOptionsMonitor<HostedServiceOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailWorkHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentValue = options.CurrentValue.EmailWork;
            var batchIsFull = await Iteration(currentValue, stoppingToken);

            if (!batchIsFull)
                await Task.Delay(currentValue.Delay, stoppingToken);
        }
    }

    private async Task<bool> Iteration(
        EmailWorkOptions opt,
        CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await sender.Send(
                new SendMailBatchCommand(opt.ScheduleAtOnce),
                ct);

            return result.Sent == opt.ScheduleAtOnce;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email batch sending failed.");
            return false;
        }
    }
}
