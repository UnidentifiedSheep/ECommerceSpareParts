using Application.Common.Interfaces.Lrt;

namespace Main.Modules.Jobs.Application.LRT;

public sealed class LrtJobAcquirer(ILrtQuotaCatalog catalog, IJobLeaseService leaseService) : ILrtJobAcquirer
{
	public async Task<IReadOnlyList<AcquiredJob>> AcquireAsync(
		TimeSpan leaseDuration,
		CancellationToken cancellationToken)
	{
		var managers = catalog.ManagersByLrt;
		var allowed = managers
			.Where(x => x.Value.AvailableQuota > 0)
			.Select(x => x.Key)
			.ToArray();

		if (allowed.Length == 0)
			return [];

		var pending = await leaseService.GetAcquirableSystemNamesAsync(allowed, cancellationToken);
		if (pending.Count == 0)
			return [];

		var acquired = new List<AcquiredJob>();
		var reserved = new Dictionary<Guid, ILrtQuotaLease>();
		try
		{
			foreach (var quota in pending.GroupBy(name => managers[name]))
			{
				while (quota.Key.TryUseQuota(Guid.NewGuid(), out var lease))
					reserved.Add(lease.HolderId, lease);

				if (reserved.Count == 0)
					continue;

				var jobs = await leaseService.AcquireJobsAsync(
					quota.ToArray(),
					reserved.Keys.ToArray(),
					leaseDuration,
					cancellationToken);

				foreach (var job in jobs)
				{
					reserved.Remove(job.LeaseHolderId!.Value, out var lease);
					acquired.Add(new AcquiredJob(job, lease!));
				}

				ReleaseAll(reserved.Values);
				reserved.Clear();
			}
		}
		catch
		{
			ReleaseAll(reserved.Values);
			ReleaseAll(acquired.Select(x => x.Lease));
			throw;
		}

		return acquired;
	}

	private static void ReleaseAll(IEnumerable<ILrtQuotaLease> leases)
	{
		foreach (var lease in leases)
			lease.Dispose();
	}
}
