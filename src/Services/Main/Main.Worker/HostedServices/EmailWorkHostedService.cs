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
            await Iteration(currentValue, stoppingToken);
        }
    }

    private async Task Iteration(
        EmailWorkOptions opt,
        CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send(
                new SendMailBatchCommand(opt.ScheduleAtOnce),
                ct);

            await Task.Delay(opt.Delay, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { logger.LogError(ex, "Email batch sending failed."); }
    }
}