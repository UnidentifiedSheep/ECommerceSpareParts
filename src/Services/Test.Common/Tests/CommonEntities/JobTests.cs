using Domain.CommonEnums;
using Domain.Exceptions;
using FluentAssertions;
using JobDomain = Domain.CommonEntities.Job;

namespace Tests.Tests.CommonEntities;

public class JobTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var job = JobDomain.Create("import-products", "{\"page\":1}", 5);

        job.SystemName.Should().Be("import-products");
        job.State.Should().Be("{\"page\":1}");
        job.MaxAttempts.Should().Be(5);
        job.Attempts.Should().Be(1);
        job.Status.Should().Be(JobStatus.Pending);
        job.ErrorMessage.Should().BeNull();
        job.LockedAt.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
        job.LeaseHolderId.Should().BeNull();
        job.IsTerminal.Should().BeFalse();
        job.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void Create_InvalidMaxAttempts_Throws()
    {
        var act = () => JobDomain.Create(
            "import-products",
            "{}",
            0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetId_ReturnsId()
    {
        var job = Create();
        var id = Guid.NewGuid();
        SetId(job, id);

        var result = job.GetId();

        result.Should().Be(id);
    }

    [Fact]
    public void GetKeySelector_ReturnsId()
    {
        var job = Create();
        var id = Guid.NewGuid();
        SetId(job, id);
        var selector = JobDomain.GetKeySelector().Compile();

        var result = selector(job);

        result.Should().Be(id);
    }

    [Fact]
    public void GetEqualityExpression_SameId_ReturnsTrue()
    {
        var job = Create();
        var id = Guid.NewGuid();
        SetId(job, id);
        var predicate = JobDomain.GetEqualityExpression(id).Compile();

        var result = predicate(job);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetEqualityExpression_DifferentId_ReturnsFalse()
    {
        var job = Create();
        SetId(job, Guid.NewGuid());
        var predicate = JobDomain.GetEqualityExpression(Guid.NewGuid()).Compile();

        var result = predicate(job);

        result.Should().BeFalse();
    }

    [Fact]
    public void AcquireLease_PendingJob_LocksAndAssignsLease()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();

        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));

        job.Status.Should().Be(JobStatus.Locked);
        job.LeaseHolderId.Should().Be(leaseHolderId);
        job.LockedAt.Should().NotBeNull();
        job.LeaseExpiresAt.Should().BeAfter(DateTime.UtcNow);
        job.ErrorMessage.Should().BeNull();
        job.Attempts.Should().Be(1);
    }

    [Fact]
    public void AcquireLease_ActiveLease_Throws()
    {
        var job = CreateLockedJob(Guid.NewGuid());

        var act = () => job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AcquireLease_ExpiredLockedJob_IncrementsAttempts()
    {
        var job = Create();
        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMilliseconds(-1));

        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));

        job.Status.Should().Be(JobStatus.Locked);
        job.Attempts.Should().Be(2);
        job.LeaseExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void AcquireLease_ExpiredProcessingJob_IncrementsAttemptsAndReturnsToLocked()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
        job.Start(leaseHolderId);
        job.RenewLease(leaseHolderId, TimeSpan.FromMilliseconds(-1));

        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));

        job.Status.Should().Be(JobStatus.Locked);
        job.Attempts.Should().Be(2);
        job.LeaseExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void AcquireLease_ExpiredLockedJobWithoutRetryAttempts_Throws()
    {
        var job = JobDomain.Create("import-products", "{}", 1);
        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMilliseconds(-1));

        var act = () => job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AcquireLease_CancellationRequestedJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);
        job.RequestCancellation();

        var act = () => job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AcquireLease_TerminalJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.Succeed(leaseHolderId);

        var act = () => job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Start_LockedJobWithActiveLease_SetsProcessing()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);

        job.Start(leaseHolderId);

        job.Status.Should().Be(JobStatus.Processing);
        job.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Start_WrongLeaseHolder_Throws()
    {
        var job = CreateLockedJob(Guid.NewGuid());

        var act = () => job.Start(Guid.NewGuid());

        act.Should().Throw<JobLeaseLostException>();
    }

    [Fact]
    public void Start_ExpiredLease_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMilliseconds(-1));

        var act = () => job.Start(leaseHolderId);

        act.Should().Throw<JobLeaseLostException>();
    }

    [Fact]
    public void SetState_ProcessingJobWithActiveLease_ChangesState()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);

        job.SetState("{\"page\":2}", leaseHolderId);

        job.State.Should().Be("{\"page\":2}");
    }

    [Fact]
    public void SetState_CancellationRequestedJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.RequestCancellation();

        var act = () => job.SetState("{\"page\":2}", leaseHolderId);

        act.Should().Throw<JobCancellationRequestedException>();
    }

    [Fact]
    public void RegisterAttempt_ActiveLease_IncrementsAttempts()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);

        job.RegisterAttempt(leaseHolderId);

        job.Attempts.Should().Be(2);
    }

    [Fact]
    public void RegisterAttempt_MaxAttemptsExceeded_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = JobDomain.Create("import-products", "{}", 1);
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));

        var act = () => job.RegisterAttempt(leaseHolderId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CanRetry_WhenAttemptsRemain_ReturnsTrue()
    {
        var job = Create();

        var result = job.CanRetry();

        result.Should().BeTrue();
    }

    [Fact]
    public void CanRetry_WhenAttemptsExhausted_ReturnsFalse()
    {
        var job = JobDomain.Create("import-products", "{}", 1);

        var result = job.CanRetry();

        result.Should().BeFalse();
    }

    [Fact]
    public void CanRetry_TerminalJob_ReturnsFalse()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.Succeed(leaseHolderId);

        var result = job.CanRetry();

        result.Should().BeFalse();
    }

    [Fact]
    public void Succeed_ProcessingJobWithActiveLease_MarksSucceededAndClearsLease()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);

        job.Succeed(leaseHolderId);

        job.Status.Should().Be(JobStatus.Succeeded);
        job.IsTerminal.Should().BeTrue();
        job.ErrorMessage.Should().BeNull();
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Succeed_LockedJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);

        var act = () => job.Succeed(leaseHolderId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Succeed_CancellationRequestedJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.RequestCancellation();

        var act = () => job.Succeed(leaseHolderId);

        act.Should().Throw<JobCancellationRequestedException>();
    }

    [Fact]
    public void Fail_ActiveJob_TrimsErrorAndClearsLease()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);

        job.Fail(leaseHolderId, "  failed  ");

        job.Status.Should().Be(JobStatus.Failed);
        job.IsTerminal.Should().BeTrue();
        job.ErrorMessage.Should().Be("failed");
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Fail_CancellationRequestedJob_MarksFailedAndClearsLease()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.RequestCancellation();

        job.Fail(leaseHolderId, "  cancelled by error  ");

        job.Status.Should().Be(JobStatus.Failed);
        job.ErrorMessage.Should().Be("cancelled by error");
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void RequestCancellation_PendingJob_CancelsImmediately()
    {
        var job = Create();

        job.RequestCancellation("  user request  ");

        job.Status.Should().Be(JobStatus.Cancelled);
        job.IsTerminal.Should().BeTrue();
        job.ErrorMessage.Should().Be("user request");
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void RequestCancellation_LockedJob_MarksCancellationRequested()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);

        job.RequestCancellation("  stop  ");

        job.Status.Should().Be(JobStatus.CancellationRequested);
        job.IsCancellationRequested.Should().BeTrue();
        job.ErrorMessage.Should().Be("stop");
        job.LeaseHolderId.Should().Be(leaseHolderId);
    }

    [Fact]
    public void RequestCancellation_ProcessingJob_MarksCancellationRequested()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);

        job.RequestCancellation("  stop  ");

        job.Status.Should().Be(JobStatus.CancellationRequested);
        job.IsCancellationRequested.Should().BeTrue();
        job.ErrorMessage.Should().Be("stop");
        job.LeaseHolderId.Should().Be(leaseHolderId);
    }

    [Fact]
    public void RequestCancellation_TerminalJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.Succeed(leaseHolderId);

        var act = () => job.RequestCancellation();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Start_CancellationRequestedJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);
        job.RequestCancellation();

        var act = () => job.Start(leaseHolderId);

        act.Should().Throw<JobCancellationRequestedException>();
    }

    [Fact]
    public void Cancel_CancellationRequestedJob_MarksCancelledAndClearsLease()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);
        job.RequestCancellation("requested");

        job.Cancel(leaseHolderId, "  cancelled  ");

        job.Status.Should().Be(JobStatus.Cancelled);
        job.IsTerminal.Should().BeTrue();
        job.ErrorMessage.Should().Be("cancelled");
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Cancel_WithoutNewError_PreservesCancellationReason()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);
        job.RequestCancellation("  requested  ");

        job.Cancel(leaseHolderId);

        job.ErrorMessage.Should().Be("requested");
    }

    [Fact]
    public void Cancel_WithoutCancellationRequest_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);

        var act = () => job.Cancel(leaseHolderId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RenewLease_ActiveLease_ExtendsLease()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);
        var previousLeaseExpiresAt = job.LeaseExpiresAt!.Value;

        job.RenewLease(leaseHolderId, TimeSpan.FromMinutes(10));

        job.LeaseExpiresAt.Should().BeAfter(previousLeaseExpiresAt);
    }

    [Fact]
    public void RenewLease_CancellationRequestedJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);
        job.RequestCancellation();

        var act = () => job.RenewLease(leaseHolderId, TimeSpan.FromMinutes(5));

        act.Should().Throw<JobCancellationRequestedException>();
    }

    [Fact]
    public void RenewLease_WrongLeaseHolder_Throws()
    {
        var job = CreateLockedJob(Guid.NewGuid());

        var act = () => job.RenewLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));

        act.Should().Throw<JobLeaseLostException>();
    }

    [Fact]
    public void EnsureActiveLease_ActiveLease_DoesNotThrow()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateLockedJob(leaseHolderId);

        var act = () => job.EnsureActiveLease(leaseHolderId);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureActiveLease_NoLease_Throws()
    {
        var job = Create();

        var act = () => job.EnsureActiveLease(Guid.NewGuid());

        act.Should().Throw<JobLeaseLostException>();
    }

    [Fact]
    public void EnsureActiveLease_ExpiredLease_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMilliseconds(-1));

        var act = () => job.EnsureActiveLease(leaseHolderId);

        act.Should().Throw<JobLeaseLostException>();
    }

    [Fact]
    public void CanBeFailedByExpiredLease_ExpiredAndAttemptsExhausted_ReturnsTrue()
    {
        var job = JobDomain.Create("import-products", "{}", 1);
        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMilliseconds(-1));

        var result = job.CanBeFailedByExpiredLease(DateTime.UtcNow);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanBeFailedByExpiredLease_PendingJob_ReturnsFalse()
    {
        var job = Create();

        var result = job.CanBeFailedByExpiredLease(DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeFailedByExpiredLease_ActiveLease_ReturnsFalse()
    {
        var job = CreateLockedJob(Guid.NewGuid());

        var result = job.CanBeFailedByExpiredLease(DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeFailedByExpiredLease_ExpiredLeaseWithAttemptsRemaining_ReturnsFalse()
    {
        var job = Create();
        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMilliseconds(-1));

        var result = job.CanBeFailedByExpiredLease(DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeFailedByExpiredLease_TerminalJob_ReturnsFalse()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.Succeed(leaseHolderId);

        var result = job.CanBeFailedByExpiredLease(DateTime.UtcNow);

        result.Should().BeFalse();
    }

    [Fact]
    public void FailByExpiredLease_ExpiredAndAttemptsExhausted_FailsAndClearsLease()
    {
        var job = JobDomain.Create("import-products", "{}", 1);
        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMilliseconds(-1));

        job.FailByExpiredLease(DateTime.UtcNow, "  lease expired  ");

        job.Status.Should().Be(JobStatus.Failed);
        job.ErrorMessage.Should().Be("lease expired");
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void FailByExpiredLease_WithoutError_UsesDefaultErrorMessage()
    {
        var job = JobDomain.Create("import-products", "{}", 1);
        job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMilliseconds(-1));

        job.FailByExpiredLease(DateTime.UtcNow);

        job.ErrorMessage.Should().Be("Job lease expired and maximum number of attempts was exceeded.");
    }

    [Fact]
    public void FailByExpiredLease_WhenLeaseIsActive_Throws()
    {
        var job = CreateLockedJob(Guid.NewGuid());

        var act = () => job.FailByExpiredLease(DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnsureStatus_WhenStatusMatches_DoesNotThrow()
    {
        var job = Create();

        var act = () => job.EnsureStatus(JobStatus.Pending);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureStatus_WhenStatusDiffers_Throws()
    {
        var job = Create();

        var act = () => job.EnsureStatus(JobStatus.Processing);

        act.Should().Throw<InvalidOperationException>();
    }

    private static JobDomain Create()
    {
        return JobDomain.Create("import-products", "{}");
    }

    private static JobDomain CreateLockedJob(Guid leaseHolderId)
    {
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
        return job;
    }

    private static JobDomain CreateProcessingJob(Guid leaseHolderId)
    {
        var job = CreateLockedJob(leaseHolderId);
        job.Start(leaseHolderId);
        return job;
    }

    private static void SetId(JobDomain job, Guid id)
    {
        typeof(JobDomain)
            .GetProperty(nameof(JobDomain.Id))!
            .SetValue(job, id);
    }
}
