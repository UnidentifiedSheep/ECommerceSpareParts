using Application.Common.Interfaces.Lrt;
using Domain.CommonEntities.Job;

namespace Application.Common.LRT;

public sealed record AcquiredJob(Job Job, ILrtQuotaLease Lease);

public sealed class LrtJobAcquirer(LrtQuotaCatalog catalog, IJobLeaseService leaseService)
{
	public async Task<AcquiredJob?> TryAcquireAsync(
		Guid holderId,
		TimeSpan leaseDuration,
		CancellationToken cancellationToken)
	{
		var managers = catalog.ManagersByLrt;
		var allowed = managers
			.Where(x => x.Value.AvailableQuota > 0)
			.Select(x => x.Key)
			.ToArray();

		if (allowed.Length == 0)
			return null;

		var job = await leaseService.TryAcquireJobAsync(holderId, leaseDuration, allowed, cancellationToken);
		if (job is null)
			return null;

		if (!managers[job.SystemName].TryUseQuota(holderId, out var lease))
			throw new InvalidOperationException(
				$"No quota available for leased job '{job.Id}' ({job.SystemName}).");

		return new AcquiredJob(job, lease);
	}
}
