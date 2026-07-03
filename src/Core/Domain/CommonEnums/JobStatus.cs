namespace Domain.CommonEnums;

public enum JobStatus
{
    Pending,
    Locked,
    Processing,
    Failed,
    Succeeded,
    CancellationRequested,
    Cancelled
}