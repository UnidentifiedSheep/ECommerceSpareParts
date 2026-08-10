using Domain.CommonEntities.Job;
using Domain.CommonEntities.Job.Events;
using Domain.CommonEnums;
using Domain.Exceptions;
using Exceptions;
using FluentAssertions;

namespace Tests.Tests.CommonEntities;

public class MultiStepJobTests
{
    [Fact]
    public void AddStep_NewStep_IsBlocked()
    {
        var job = Create();
        var step = AddStep(job);

        step.Status.Should().Be(JobStatus.Blocked);
        step.IsStep.Should().BeTrue();
        step.MultiStepJobId.Should().Be(job.Id);
        step.MultiStepJob.Should().BeSameAs(job);
        job.Steps.Should().ContainSingle().Which.Should().BeSameAs(step);
    }

    [Fact]
    public void AddStep_NestedMultiStepJob_IsValidStep()
    {
        var parent = Create();
        var nested = MultiStepJob.Create("nested", "{}");

        parent.AddStep(nested);

        nested.Status.Should().Be(JobStatus.Blocked);
        nested.IsStep.Should().BeTrue();
        nested.MultiStepJobId.Should().Be(parent.Id);
        parent.Steps.Should().ContainSingle().Which.Should().BeSameAs(nested);
    }

    [Fact]
    public void AddDependency_OwnsGraphEdge()
    {
        var job = Create();
        var step = AddStep(job);
        var dependsOn = SingleRunJob.Create("dependency", "{}");
        job.AddStep(dependsOn);

        job.AddDependency(step, dependsOn);

        var dependency = job.Dependencies.Should().ContainSingle().Which;
        dependency.MultiStepJobId.Should().Be(job.Id);
        dependency.MultiStepJob.Should().BeSameAs(job);
        dependency.Step.Should().BeSameAs(step);
        dependency.DependsOnStep.Should().BeSameAs(dependsOn);
    }

    [Fact]
    public void ActivateStep_BlockedStep_MovesToPending()
    {
        var job = Create();
        var step = AddStep(job);

        job.ActivateStep(step);

        step.Status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public void CancelUnfinishedSteps_CancelsEveryNonTerminalStep()
    {
        var job = Create();
        var blocked = AddStep(job);
        var pending = AddStep(job);
        var processing = AddStep(job);
        var succeeded = AddStep(job);
        job.ActivateStep(pending);
        job.ActivateStep(processing);
        job.ActivateStep(succeeded);

        var processingLeaseHolderId = Guid.NewGuid();
        processing.AcquireLease(
            processingLeaseHolderId,
            TimeSpan.FromMinutes(5));
        processing.Start(processingLeaseHolderId);

        var succeededLeaseHolderId = Guid.NewGuid();
        succeeded.AcquireLease(
            succeededLeaseHolderId,
            TimeSpan.FromMinutes(5));
        succeeded.Start(succeededLeaseHolderId);
        succeeded.Succeed(succeededLeaseHolderId);

        job.CancelUnfinishedSteps(
            job.Steps,
            "workflow failed");

        blocked.Status.Should().Be(JobStatus.Cancelled);
        pending.Status.Should().Be(JobStatus.Cancelled);
        processing.Status.Should().Be(JobStatus.Cancelled);
        processing.LeaseHolderId.Should().BeNull();
        processing.LeaseExpiresAt.Should().BeNull();
        succeeded.Status.Should().Be(JobStatus.Succeeded);
    }

    [Fact]
    public void CancelUnfinishedSteps_ForeignStep_Throws()
    {
        var job = Create();
        var foreignStep = AddStep(Create());

        var act = () => job.CancelUnfinishedSteps([foreignStep]);

        act.Should().Throw<InvalidOperationException>();
        foreignStep.Status.Should().Be(JobStatus.Blocked);
    }

    [Fact]
    public void AcquireLease_BlockedStep_Throws()
    {
        var step = AddStep(Create());

        var act = () => step.AcquireLease(
            Guid.NewGuid(),
            TimeSpan.FromMinutes(5));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ActivateStep_NestedMultiStepAlreadyWaiting_Throws()
    {
        var parent = Create();
        var nested = MultiStepJob.Create("nested", "{}");
        parent.AddStep(nested);
        parent.ActivateStep(nested);
        var leaseHolderId = Guid.NewGuid();
        nested.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
        nested.Start(leaseHolderId);
        nested.Wait(leaseHolderId);

        var act = () => parent.ActivateStep(nested);

        act.Should().Throw<InvalidOperationException>();
        nested.Status.Should().Be(JobStatus.Waiting);
    }

    [Fact]
    public void AddStep_ActivatedNestedMultiStepJob_Throws()
    {
        var parent = Create();
        var nested = MultiStepJob.Create("nested", "{}");
        parent.AddStep(nested);
        parent.ActivateStep(nested);

        var act = () => nested.AddStep(
            SingleRunJob.Create("late-step", "{}"));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Nested multi-step job topology cannot be changed.");
        nested.Status.Should().Be(JobStatus.Pending);
        nested.Steps.Should().BeEmpty();
    }

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

        var act = () => job.AddStep(SingleRunJob.Create("another-step", "{}"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StepSucceeded_RaisesFinishedEvent()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();
        var step = AddStep(job);
        job.ActivateStep(step);
        step.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
        step.Start(leaseHolderId);

        step.Succeed(leaseHolderId);
        step.OnUpdated();

        var @event = step.FlushDomainEvents()
            .OfType<JobStepFinishedDomainEvent>()
            .Should().ContainSingle()
            .Which;
        @event.JobStepId.Should().Be(step.Id);
        @event.MultiStepJobId.Should().Be(job.Id);
        @event.Status.Should().Be(JobStatus.Succeeded);
    }

    [Fact]
    public void StepFailed_RaisesFinishedEvent()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = Create();
        var step = AddStep(job);
        job.ActivateStep(step);
        step.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));

        step.Fail(leaseHolderId, "failed");
        step.OnUpdated();

        step.FlushDomainEvents()
            .OfType<JobStepFinishedDomainEvent>()
            .Should().ContainSingle()
            .Which.Status.Should().Be(JobStatus.Failed);
    }

    [Fact]
    public void RequestCancellation_Step_Throws()
    {
        var job = Create();
        var step = AddStep(job);

        var act = () => step.RequestCancellation();

        act.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("job.step.cannot.be.cancelled.directly");
        step.Status.Should().Be(JobStatus.Blocked);
    }

    [Fact]
    public void NonTerminalStepUpdate_DoesNotRaiseFinishedEvent()
    {
        var job = Create();
        var step = AddStep(job);

        step.OnUpdated();

        step.FlushDomainEvents()
            .OfType<JobStepFinishedDomainEvent>()
            .Should().BeEmpty();
    }

    private static MultiStepJob Create()
    {
        return MultiStepJob.Create("multi-step", "{}");
    }

    private static Job AddStep(MultiStepJob parent)
    {
        var step = SingleRunJob.Create("step", "{}");
        parent.AddStep(step);
        return step;
    }

    private static MultiStepJob CreateProcessingJob(Guid leaseHolderId)
    {
        var job = Create();
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
        job.Start(leaseHolderId);
        return job;
    }
}
