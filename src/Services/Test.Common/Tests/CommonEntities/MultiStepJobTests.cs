using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using Domain.Exceptions;
using FluentAssertions;

namespace Tests.Tests.CommonEntities;

public class MultiStepJobTests
{
    [Fact]
    public void Wait_ProcessingJob_MovesToWaitingAndClearsLease()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);

        job.Wait(leaseHolderId);

        job.Status.Should().Be(JobStatus.Waiting);
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Wait_LockedJob_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));

        var act = () => job.Wait(leaseHolderId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Wait_WithAnotherLeaseHolder_Throws()
    {
        var job = CreateProcessingJob(Guid.NewGuid());

        var act = () => job.Wait(Guid.NewGuid());

        act.Should().Throw<JobLeaseLostException>();
    }

    [Fact]
    public void Wait_WithExpiredLease_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMilliseconds(-1));

        var act = () => job.Wait(leaseHolderId);

        act.Should().Throw<JobLeaseLostException>();
    }

    [Fact]
    public void Wait_WhenCancellationRequested_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.RequestCancellation();

        var act = () => job.Wait(leaseHolderId);

        act.Should().Throw<JobCancellationRequestedException>();
    }

    [Fact]
    public void Resume_WaitingJob_MovesToPending()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.Wait(leaseHolderId);

        job.Resume();

        job.Status.Should().Be(JobStatus.Pending);
        job.LeaseHolderId.Should().BeNull();
        job.LeaseExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Resume_NonWaitingJob_Throws()
    {
        var job = Create();

        var act = job.Resume;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddStep_AfterResume_Throws()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = CreateProcessingJob(leaseHolderId);
        job.Wait(leaseHolderId);
        job.Resume();

        var act = () => job.AddStep("another-step", "{}");

        act.Should().Throw<InvalidOperationException>();
    }

    private static MultiStepJob Create()
    {
        return MultiStepJob.Create("multi-step", "{}");
    }

    private static MultiStepJob CreateProcessingJob(Guid leaseHolderId)
    {
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
        job.Start(leaseHolderId);
        return job;
    }
}
