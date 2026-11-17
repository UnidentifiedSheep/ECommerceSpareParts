using Application.Common.Interfaces.Persistence;
using Domain.CommonEntities.Job;

namespace Application.Common.Interfaces.Lrt;

public interface IJobStepInputInitializer
{
	Task<Job> InitializeAsync(
		Job step,
		Guid leaseHolderId,
		IApplicationTransactionContext context,
		CancellationToken cancellationToken);
}
