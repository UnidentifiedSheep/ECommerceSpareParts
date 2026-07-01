using System.Linq.Expressions;
using Domain.CommonEnums;
using Domain.Extensions;
using Domain.Interfaces;

namespace Domain.CommonEntities;

public class Job : AuditableEntity<Job, Guid>, ILinqEntity<Job, Guid>
{
    protected Job(
        string systemName,
        int maxAttempts)
    {
        Status = JobStatus.Pending;
        SystemName = systemName;
        SetMaxAttempts(maxAttempts);
    }

    public Guid Id { get; private set; }
    public string SystemName { get; private set; }
    public string State { get; protected set; } = string.Empty;
    public JobStatus Status { get; protected set; }
    public int Attempts { get; private set; }
    public int MaxAttempts { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? LockedAt { get; private set; }

    public bool IsTerminal => Status is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled;

    public static Expression<Func<Job, Guid>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<Job, bool>> GetEqualityExpression(Guid key) { return x => x.Id == key; }

    public override Guid GetId() { return Id; }

    public static Job Create(string systemName, int maxAttempts = 3)
    {
        return new Job(systemName, maxAttempts);
    }

    public bool CanRetry()
    {
        if (IsTerminal) return false;
        return Attempts < MaxAttempts;
    }

    public void RegisterAttempt()
    {
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot have new attempts.");
        if (Attempts >= MaxAttempts)
            throw new InvalidOperationException("Maximum number of attempts exceeded.");

        Attempts++;
    }

    public void SetState(string state)
    {
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot have new state.");
        State = state;
    }

    public void EnsureStatus(JobStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Job must be in {expected} status, but current status is {Status}.");
    }

    public void SetMaxAttempts(int maxAttempts)
    {
        MaxAttempts = maxAttempts.AgainstLessOrEqual(
            0,
            () => throw new InvalidOperationException("job.max.attempts.must.be.greater.than.zero"));
    }

    public void Start()
    {
        EnsureStatus(JobStatus.Locked);
        ErrorMessage = null;
        Status = JobStatus.Processing;
    }

    public void Succeed()
    {
        EnsureStatus(JobStatus.Processing);
        ErrorMessage = null;
        Status = JobStatus.Succeeded;
    }

    public void Fail(string? errorMessage)
    {
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot be failed.");

        ErrorMessage = errorMessage?.TrimOrNull();
        Status = JobStatus.Failed;
    }

    public void Cancel(string? errorMessage = null)
    {
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot be cancelled.");

        ErrorMessage = errorMessage?.TrimOrNull();
        Status = JobStatus.Cancelled;
    }

    public void Lock()
    {
        if (LockedAt != null) throw new InvalidOperationException("Job is already locked.");

        LockedAt = DateTime.UtcNow;
        Status = JobStatus.Locked;
    }
}