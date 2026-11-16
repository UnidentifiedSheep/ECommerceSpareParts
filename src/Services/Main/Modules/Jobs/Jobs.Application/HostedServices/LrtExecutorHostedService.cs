using Application.Common.Interfaces;
using Application.Common.Interfaces.Lrt;
using Main.Modules.Jobs.Application.LRT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Jobs.Application.HostedServices;

public class LrtExecutorHostedService(
	IServiceScopeFactory scopeFactory,
	LrtQuotaCatalog quotaCatalog,
	IJobSignal jobSignal,
	ILogger<LrtExecutorHostedService> logger) : BackgroundService
{
	private static readonly TimeSpan InitialLeaseDuration = TimeSpan.FromMinutes(5);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		quotaCatalog.Validate();

		while (!stoppingToken.IsCancellationRequested)
		{
			var jobStarted = await Iteration(stoppingToken);

			if (!jobStarted)
				await jobSignal.WaitAsync(JobSignal.PollInterval, stoppingToken);
		}
	}

	private async Task<bool> Iteration(CancellationToken ct)
	{
		try
		{
			await using var scope = scopeFactory.CreateAsyncScope();
			var acquirer = scope.ServiceProvider.GetRequiredService<LrtJobAcquirer>();

			var acquired = await acquirer.TryAcquireAsync(Guid.NewGuid(), InitialLeaseDuration, ct);
			if (acquired is null)
				return false;

			_ = RunJobAndReleaseQuota(
				acquired.Job.Id,
				acquired.Job.SystemName,
				acquired.Lease,
				ct);
			return true;
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Job execution failed.");
			return false;
		}
	}

	private async Task RunJobAndReleaseQuota(
		Guid jobId,
		string systemName,
		ILrtQuotaLease lrtQuota,
		CancellationToken ct)
	{
		using (lrtQuota)
		{
			try
			{
				await using var scope = scopeFactory.CreateAsyncScope();

				var registry =
					scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtNamedObject>>();

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
