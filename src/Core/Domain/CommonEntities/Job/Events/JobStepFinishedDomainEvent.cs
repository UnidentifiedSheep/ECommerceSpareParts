using Domain.CommonEnums;
using Domain.Interfaces.Events;

namespace Domain.CommonEntities.Job.Events;

public sealed record JobStepFinishedDomainEvent(
    Guid JobStepId,
    Guid MultiStepJobId,
    JobStatus Status) : IKeyedDomainEvent, IBatchableDomainEvent
{
    public string GetKey() => $"multi-step-job:{MultiStepJobId}:resume";
}
