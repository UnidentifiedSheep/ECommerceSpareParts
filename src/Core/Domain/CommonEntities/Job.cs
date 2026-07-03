using System.Linq.Expressions;
using Domain.CommonEnums;
using Domain.Exceptions;
using Domain.Extensions;
using Domain.Interfaces;

namespace Domain.CommonEntities;

public class Job : AuditableEntity<Job, Guid>, ILinqEntity<Job, Guid>
{
    private Job() { }
    protected Job(
        string systemName,
        string initialState,
        int maxAttempts)
    {
        Status = JobStatus.Pending;
        SystemName = systemName;
        Attempts = 1;
        SetMaxAttempts(maxAttempts);
        State = initialState;
    }

    public Guid Id { get; private set; }
    public string SystemName { get; private set; } = null!;
    public string State { get; protected set; } = string.Empty;
    public JobStatus Status { get; protected set; }
    public int Attempts { get; private set; }
    public int MaxAttempts { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? LockedAt { get; private set; }
    public DateTime? LeaseExpiresAt { get; private set; }
    public Guid? LeaseHolderId { get; private set; }

    public bool IsTerminal => Status is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled;
    public bool IsCancellationRequested => Status == JobStatus.CancellationRequested;
    public bool HasActiveLease => LeaseExpiresAt is not null && LeaseExpiresAt > DateTime.UtcNow;

    public static Expression<Func<Job, Guid>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<Job, bool>> GetEqualityExpression(Guid key) { return x => x.Id == key; }

    public override Guid GetId() { return Id; }

    public static Job Create(
        string systemName,
        string initialState,
        int maxAttempts = 3)
    {
        return new Job(systemName, initialState, maxAttempts);
    }

    public bool CanRetry()
    {
        if (IsTerminal) return false;
        return Attempts < MaxAttempts;
    }

    public void RegisterAttempt(Guid leaseHolderId)
    {
        EnsureActiveLease(leaseHolderId);
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot have new attempts.");
        if (Attempts >= MaxAttempts)
            throw new InvalidOperationException("Maximum number of attempts exceeded.");

        Attempts++;
    }

    public void SetState(string state, Guid leaseHolderId)
    {
        EnsureActiveLease(leaseHolderId);
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot have new state.");
        if (IsCancellationRequested) throw new JobCancellationRequestedException(Id);
        State = state;
    }

    public void EnsureStatus(JobStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Job must be in {expected} status, but current status is {Status}.");
    }

    private void SetMaxAttempts(int maxAttempts)
    {
        MaxAttempts = maxAttempts.AgainstLessOrEqual(
            0,
            () => throw new InvalidOperationException("job.max.attempts.must.be.greater.than.zero"));
    }

    public void Start(Guid leaseHolderId)
    {
        EnsureActiveLease(leaseHolderId);
        
        if (IsCancellationRequested) throw new JobCancellationRequestedException(Id);
        
        EnsureStatus(JobStatus.Locked);
        
        ErrorMessage = null;
        Status = JobStatus.Processing;
    }

    public void Succeed(Guid leaseHolderId)
    {
        EnsureActiveLease(leaseHolderId);
        
        if (IsCancellationRequested) throw new JobCancellationRequestedException(Id);
        
        EnsureStatus(JobStatus.Processing);
        
        ErrorMessage = null;
        Status = JobStatus.Succeeded;
        ClearLease();
    }

    public void Fail(Guid leaseHolderId, string? errorMessage)
    {
        EnsureActiveLease(leaseHolderId);
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot be failed.");

        ErrorMessage = errorMessage?.TrimOrNull();
        Status = JobStatus.Failed;
        ClearLease();
    }

    public void Cancel(Guid leaseHolderId, string? errorMessage = null)
    {
        EnsureActiveLease(leaseHolderId);

        if (IsTerminal)
            throw new InvalidOperationException("Terminal job cannot be cancelled.");

        if (!IsCancellationRequested)
            throw new InvalidOperationException("Job cancellation was not requested.");

        ErrorMessage = errorMessage?.TrimOrNull() ?? ErrorMessage;
        Status = JobStatus.Cancelled;
        ClearLease();
    }
    
    public void RequestCancellation(string? reason = null)
    {
        if (IsTerminal)
            throw new InvalidOperationException("Terminal job cannot be cancelled.");

        ErrorMessage = reason?.TrimOrNull();

        if (Status == JobStatus.Pending)
        {
            Status = JobStatus.Cancelled;
            ClearLease();
            return;
        }

        Status = JobStatus.CancellationRequested;
    }
    
    public void Lock(Guid leaseHolderId, TimeSpan leaseDuration)
    {
        if (IsTerminal)
            throw new InvalidOperationException("Terminal job cannot be locked.");
        
        if (HasActiveLease)
            throw new InvalidOperationException("Job already has active lease.");

        var now = DateTime.UtcNow;

        LockedAt = now;
        Status = JobStatus.Locked;
        LeaseExpiresAt = now.Add(leaseDuration);
        LeaseHolderId = leaseHolderId;
    }

    public void RenewLease(Guid leaseHolderId, TimeSpan leaseDuration)
    {
        EnsureActiveLease(leaseHolderId);
        if (IsTerminal) throw new InvalidOperationException("Terminal job cannot be updated.");
        if (IsCancellationRequested) throw new JobCancellationRequestedException(Id);
        LeaseExpiresAt = DateTime.UtcNow.Add(leaseDuration);
    }
    
    public void EnsureActiveLease(Guid leaseHolderId)
    {
        if (LeaseHolderId != leaseHolderId)
            throw new JobLeaseLostException(Id);

        if (LeaseExpiresAt is null || LeaseExpiresAt <= DateTime.UtcNow)
            throw new JobLeaseLostException(Id);
    }
    
    private void ClearLease()
    {
        LeaseHolderId = null;
        LeaseExpiresAt = null;
    }
}