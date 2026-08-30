using System.Linq.Expressions;
using Domain.CommonEntities.Job.Events;
using Domain.CommonEnums;
using Domain.Exceptions;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Exceptions;

namespace Domain.CommonEntities.Job;

public abstract class Job : AuditableEntity<Job, Guid>, ILinqEntity<Job, Guid>
{
	protected Job()
	{
	}

	protected Job(
		string systemName,
		string initialState,
		int maxAttempts,
		string? naturalKey = null)
	{
		Id = Guid.NewGuid();
		SystemName = systemName;
		SetNaturalKey(naturalKey);
		Attempts = 1;
		SetMaxAttempts(maxAttempts);
		State = initialState;
		SetStatus(JobStatus.Pending);
	}

	public Guid Id { get; }

	public string SystemName { get; private set; } = null!;

	public string? NaturalKey { get; private set; }

	public string State { get; protected set; } = string.Empty;

	public JobStatus Status { get; private set; }

	public int Attempts { get; private set; }

	public int MaxAttempts { get; private set; }

	public string? ErrorMessage { get; private set; }

	public DateTime? LockedAt { get; private set; }

	public DateTime? LeaseExpiresAt { get; private set; }

	public Guid? LeaseHolderId { get; private set; }

	public Guid? MultiStepJobId { get; private set; }

	public MultiStepJob? MultiStepJob { get; private set; }

	public bool IsTerminal => Status is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled;

	public bool IsCancellationRequested => Status == JobStatus.CancellationRequested;

	public bool IsStep => MultiStepJobId.HasValue;

	public static Expression<Func<Job, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<Job, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;

	public override Guid GetId() => Id;

	public virtual bool CanRetry()
	{
		if (IsTerminal)
			return false;
		return Attempts < MaxAttempts;
	}

	public void RegisterAttempt(Guid leaseHolderId)
	{
		EnsureActiveLease(leaseHolderId);
		if (IsTerminal)
			throw new InvalidOperationException("Terminal job cannot have new attempts.");
		if (Attempts >= MaxAttempts)
			throw new InvalidOperationException("Maximum number of attempts exceeded.");

		Attempts++;
		RaiseStatusUpdatedEvent();
	}

	public void SetState(string state, Guid leaseHolderId)
	{
		EnsureActiveLease(leaseHolderId);
		if (IsTerminal)
			throw new InvalidOperationException("Terminal job cannot have new state.");
		if (IsCancellationRequested)
			throw new JobCancellationRequestedException(Id);
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
		MaxAttempts = maxAttempts.EnsureGreaterThan(
			0,
			() => throw new InvalidOperationException("job.max.attempts.must.be.greater.than.zero"));
	}

	private void SetNaturalKey(string? naturalKey)
	{
		NaturalKey = naturalKey is null
			? null
			: naturalKey
				.TrimSafe()
				.EnsureNotNullOrWhiteSpace(() =>
					new InvalidOperationException("Natural key cannot be null or empty."));
	}

	public virtual void Start(Guid leaseHolderId)
	{
		EnsureActiveLease(leaseHolderId);

		if (IsCancellationRequested)
			throw new JobCancellationRequestedException(Id);

		EnsureStatus(JobStatus.Locked);

		ErrorMessage = null;
		SetStatus(JobStatus.Processing);
	}

	public virtual void Succeed(Guid leaseHolderId)
	{
		EnsureActiveLease(leaseHolderId);

		if (IsCancellationRequested)
			throw new JobCancellationRequestedException(Id);

		EnsureStatus(JobStatus.Processing);

		ErrorMessage = null;
		SetStatus(JobStatus.Succeeded);
		ClearLease();
	}

	public virtual void Fail(Guid leaseHolderId, string? errorMessage)
	{
		EnsureActiveLease(leaseHolderId);
		if (IsTerminal)
			throw new InvalidOperationException("Terminal job cannot be failed.");

		ErrorMessage = errorMessage?.TrimOrNull();
		SetStatus(JobStatus.Failed);
		ClearLease();
	}

	public virtual void Cancel(Guid leaseHolderId, string? errorMessage = null)
	{
		EnsureActiveLease(leaseHolderId);

		if (IsTerminal)
			throw new InvalidOperationException("Terminal job cannot be cancelled.");

		if (!IsCancellationRequested)
			throw new InvalidOperationException("Job cancellation was not requested.");

		ErrorMessage = errorMessage?.TrimOrNull() ?? ErrorMessage;
		SetStatus(JobStatus.Cancelled);
		ClearLease();
	}

	public virtual void RequestCancellation(string? reason = null)
	{
		if (IsStep)
			throw new InvalidInputException("job.step.cannot.be.cancelled.directly");

		if (IsTerminal)
			throw new InvalidOperationException("Terminal job cannot be cancelled.");

		ErrorMessage = reason?.TrimOrNull();

		if (Status is JobStatus.Pending or JobStatus.Waiting or JobStatus.Blocked)
		{
			SetStatus(JobStatus.Cancelled);
			ClearLease();
			return;
		}

		SetStatus(JobStatus.CancellationRequested);
	}

	public void AcquireLease(Guid leaseHolderId, TimeSpan leaseDuration)
	{
		if (IsTerminal)
			throw new InvalidOperationException("Terminal job cannot be acquired.");

		if (Status == JobStatus.CancellationRequested)
			throw new InvalidOperationException("Cancellation requested job cannot be acquired.");

		if (Status is not JobStatus.Pending and not JobStatus.Locked and not JobStatus.Processing)
			throw new InvalidOperationException($"Job in {Status} status cannot be acquired.");

		var now = DateTime.UtcNow;

		if (LeaseExpiresAt > now)
			throw new InvalidOperationException("Job already has active lease.");

		if (Status is JobStatus.Locked or JobStatus.Processing)
		{
			if (!CanRetry())
				throw new InvalidOperationException("Maximum number of attempts exceeded.");

			Attempts++;
		}

		LockedAt = now;
		SetStatus(JobStatus.Locked);
		ErrorMessage = null;
		LeaseExpiresAt = now.Add(leaseDuration);
		LeaseHolderId = leaseHolderId;
	}

	public void RenewLease(Guid leaseHolderId, TimeSpan leaseDuration)
	{
		EnsureActiveLease(leaseHolderId);
		if (IsTerminal)
			throw new InvalidOperationException("Terminal job cannot be updated.");
		if (IsCancellationRequested)
			throw new JobCancellationRequestedException(Id);
		LeaseExpiresAt = DateTime.UtcNow.Add(leaseDuration);
	}

	public virtual bool CanBeFailedByExpiredLease(DateTime now)
	{
		if (IsTerminal)
			return false;

		if (Status is not JobStatus.Locked and not JobStatus.Processing)
			return false;

		if (LeaseExpiresAt is null || LeaseExpiresAt > now)
			return false;

		return Attempts >= MaxAttempts;
	}

	public virtual void FailByExpiredLease(DateTime now, string? errorMessage = null)
	{
		if (!CanBeFailedByExpiredLease(now))
			throw new InvalidOperationException("Job cannot be failed by expired lease.");

		ErrorMessage = errorMessage?.TrimOrNull() ??
			"Job lease expired and maximum number of attempts was exceeded.";

		SetStatus(JobStatus.Failed);
		ClearLease();
	}

	public void EnsureActiveLease(Guid leaseHolderId)
	{
		if (LeaseHolderId != leaseHolderId)
			throw new JobLeaseLostException(Id);

		if (LeaseExpiresAt is null || LeaseExpiresAt <= DateTime.UtcNow)
			throw new JobLeaseLostException(Id);
	}

	protected void ClearLease()
	{
		LeaseHolderId = null;
		LeaseExpiresAt = null;
	}

	internal void AttachTo(MultiStepJob parent)
	{
		ArgumentNullException.ThrowIfNull(parent);
		EnsureStatus(JobStatus.Pending);

		if (IsStep)
			throw new InvalidOperationException("Job already belongs to a multi-step job.");

		MultiStepJobId = parent.Id;
		MultiStepJob = parent;
		SetStatus(JobStatus.Blocked);
	}

	internal void Activate(Guid multiStepJobId)
	{
		if (MultiStepJobId != multiStepJobId)
			throw new InvalidOperationException("Job does not belong to the specified multi-step job.");

		EnsureStatus(JobStatus.Blocked);
		SetStatus(JobStatus.Pending);
	}

	internal void CancelBy(MultiStepJob parent, string? reason = null)
	{
		ArgumentNullException.ThrowIfNull(parent);

		if (MultiStepJobId != parent.Id)
			throw new InvalidOperationException("Job does not belong to the specified multi-step job.");

		if (IsTerminal)
			return;

		ErrorMessage = reason?.TrimOrNull();
		SetStatus(JobStatus.Cancelled);
		ClearLease();
	}

	protected void SetStatus(JobStatus status)
	{
		Status = status;
		RaiseStatusUpdatedEvent();
	}

	private void RaiseStatusUpdatedEvent()
	{
		AddDomainEvent(
			new JobStatusUpdatedDomainEvent(
				Id,
				Status,
				Attempts));
	}

	public override void OnCreated() => RaiseFinishedEventIfStepIsTerminal();

	public override void OnUpdated() => RaiseFinishedEventIfStepIsTerminal();

	private void RaiseFinishedEventIfStepIsTerminal()
	{
		if (!IsTerminal || !MultiStepJobId.HasValue)
			return;

		AddDomainEvent(
			new JobStepFinishedDomainEvent(
				Id,
				MultiStepJobId.Value,
				Status));
	}
}
