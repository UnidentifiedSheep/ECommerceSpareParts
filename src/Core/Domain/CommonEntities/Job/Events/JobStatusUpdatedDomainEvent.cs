using Domain.CommonEnums;
using Domain.Interfaces.Events;

namespace Domain.CommonEntities.Job.Events;

public sealed record JobStatusUpdatedDomainEvent(Guid JobId, JobStatus Status, int CurrentAttempt)
	: IKeyedDomainEvent, IBatchableDomainEvent
{
	public string GetKey() => $"job-status-updated:{JobId}";
}
