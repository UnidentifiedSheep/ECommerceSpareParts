using Application.Common.Interfaces.Lrt;
using Application.Common.Models.Options;
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
			var batchIsFull = await Iteration(currentValue, stoppingToken);

			if (!batchIsFull)
				await Task.Delay(currentValue.Delay, stoppingToken);
		}
	}

	private async Task<bool> Iteration(LrtExecutorOptions opt, CancellationToken ct)
	{
		try
		{
			await using var scope = scopeFactory.CreateAsyncScope();
			var leaseService = scope.ServiceProvider.GetRequiredService<IJobLeaseService>();
			var cancelledJobs = await leaseService.CancelExpiredJobs(
				opt.MaxExpiredLeaseFailBatchSize,
				ct);

			if (cancelledJobs.Count > 0)
				logger.LogInformation(
					"Jobs with expired leases and cancellation requests are made 'Cancelled'. Count: {Count}",
					cancelledJobs.Count);

			var jobs = await leaseService.FailExpiredJobsWithoutAttempts(
				opt.MaxExpiredLeaseFailBatchSize,
				ct);

			if (jobs.Count > 0)
				logger.LogInformation(
					"Jobs with expired leases are made 'Failed'. Count: {Count}",
					jobs.Count);

			return cancelledJobs.Count == opt.MaxExpiredLeaseFailBatchSize ||
				jobs.Count == opt.MaxExpiredLeaseFailBatchSize;
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Expired job wipe failed.");
			return false;
		}
	}
}
