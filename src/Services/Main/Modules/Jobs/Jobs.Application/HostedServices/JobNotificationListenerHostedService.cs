using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Listening;

namespace Main.Modules.Jobs.Application.HostedServices;

public class JobNotificationListenerHostedService(
	IChannelListenerFactory listeners,
	IJobSignal signal,
	ILogger<JobNotificationListenerHostedService> logger) : BackgroundService
{
	private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
			try
			{
				await using var listener = await listeners.ListenAsync(JobSignal.Channel, stoppingToken);
				await foreach (var _ in listener.ReadAllAsync(stoppingToken))
					signal.Notify();
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				return;
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Job notification listener failed. Falling back to polling and retrying.");
				await Task.Delay(RetryDelay, stoppingToken);
			}
	}
}
