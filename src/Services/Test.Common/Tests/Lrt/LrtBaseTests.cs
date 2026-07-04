using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Abstractions.Models;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Contracts.Job;
using Domain.CommonEntities;
using Domain.CommonEnums;
using Domain.Exceptions;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Stubs;

namespace Tests.Tests.Lrt;

public class LrtBaseTests
{
    [Fact]
    public async Task ExecuteAsync_ValidJob_StartsWorkAndMarksSucceeded()
    {
        var fixture = CreateFixture();
        var lrt = fixture.CreateLrt();

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        lrt.DoWorkCalls.Should().Be(1);
        fixture.Job.Status.Should().Be(JobStatus.Succeeded);
        fixture.JobStatusEvents.Select(x => x.Status)
            .Should().Equal(JobStatus.Processing.ToString(), JobStatus.Succeeded.ToString());
        fixture.JobStatusEvents.Select(x => x.CurrentAttempt)
            .Should().Equal(1, 1);
        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_DoWorkFailsWithAttemptsRemaining_RetriesAndThenSucceeds()
    {
        var fixture = CreateFixture(maxAttempts: 3);
        var lrt = fixture.CreateLrt();
        lrt.Work = x =>
        {
            if (x.DoWorkCalls == 1)
                throw new InvalidOperationException("temporary failure");

            return Task.CompletedTask;
        };

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        lrt.DoWorkCalls.Should().Be(2);
        fixture.Job.Status.Should().Be(JobStatus.Succeeded);
        fixture.Job.Attempts.Should().Be(2);
        fixture.JobStatusEvents.Select(x => x.Status)
            .Should().Equal(
                JobStatus.Processing.ToString(),
                JobStatus.Processing.ToString(),
                JobStatus.Succeeded.ToString());
        fixture.JobStatusEvents.Select(x => x.CurrentAttempt)
            .Should().Equal(1, 2, 2);
    }

    [Fact]
    public async Task ExecuteAsync_DoWorkFailsWithoutAttemptsRemaining_MarksFailed()
    {
        var fixture = CreateFixture(maxAttempts: 1);
        var lrt = fixture.CreateLrt();
        lrt.Work = _ => throw new InvalidOperationException("permanent failure");

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        lrt.DoWorkCalls.Should().Be(1);
        fixture.Job.Status.Should().Be(JobStatus.Failed);
        fixture.Job.ErrorMessage.Should().Be("permanent failure");
        fixture.JobStatusEvents.Select(x => x.Status)
            .Should().Equal(JobStatus.Processing.ToString(), JobStatus.Failed.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_Interrupted_MarksFailedWithoutRetry()
    {
        var fixture = CreateFixture(maxAttempts: 3);
        var lrt = fixture.CreateLrt();
        lrt.Work = x =>
        {
            x.InterruptForTest("manual stop");
            return Task.CompletedTask;
        };

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        fixture.Job.Status.Should().Be(JobStatus.Failed);
        fixture.Job.Attempts.Should().Be(1);
        fixture.Job.ErrorMessage.Should().Be("manual stop");
        fixture.JobStatusEvents.Select(x => x.Status)
            .Should().Equal(JobStatus.Processing.ToString(), JobStatus.Failed.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_StopsWithoutFailingJob()
    {
        var fixture = CreateFixture();
        var lrt = fixture.CreateLrt();
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        lrt.Work = _ => throw new OperationCanceledException(cancellationTokenSource.Token);

        await lrt.ExecuteAsync(
            fixture.JobId,
            fixture.LeaseHolderId,
            cancellationTokenSource.Token);

        fixture.Job.Status.Should().Be(JobStatus.Processing);
        fixture.JobStatusEvents.Select(x => x.Status)
            .Should().Equal(JobStatus.Processing.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenJobCancellationRequested_CancelsJob()
    {
        var fixture = CreateFixture();
        var lrt = fixture.CreateLrt();
        lrt.Work = x =>
        {
            x.RequestCancellationForTest("cancel requested");
            throw new JobCancellationRequestedException(fixture.JobId);
        };

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        fixture.Job.Status.Should().Be(JobStatus.Cancelled);
        fixture.Job.ErrorMessage.Should().Be("cancel requested");
        fixture.JobStatusEvents.Select(x => x.Status)
            .Should().Equal(JobStatus.Processing.ToString(), JobStatus.Cancelled.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLeaseLost_StopsWithoutFailingJob()
    {
        var fixture = CreateFixture();
        var lrt = fixture.CreateLrt();
        lrt.Work = _ => throw new JobLeaseLostException(fixture.JobId);

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        fixture.Job.Status.Should().Be(JobStatus.Processing);
        fixture.JobStatusEvents.Select(x => x.Status)
            .Should().Equal(JobStatus.Processing.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_DoWorkUpdatesState_PersistsStateAndRenewsLease()
    {
        var fixture = CreateFixture();
        var lrt = fixture.CreateLrt();
        DateTime? renewedLeaseExpiresAt = null;
        lrt.Work = async x =>
        {
            await x.UpdateStateForTest(new TestState { Value = 42 });
            renewedLeaseExpiresAt = x.CurrentLeaseExpiresAt;
        };

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        fixture.Job.State.Should().Be("""{"Value":42}""");
        renewedLeaseExpiresAt.Should().NotBeNull();
        fixture.Job.Status.Should().Be(JobStatus.Succeeded);
        fixture.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteAsync_DoWorkReadsState_DeserializesCurrentJobState()
    {
        var fixture = CreateFixture("""{"Value":7}""");
        var lrt = fixture.CreateLrt();
        lrt.Work = x => x.CaptureStateForTest();

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        lrt.CapturedState.Should().NotBeNull();
        lrt.CapturedState!.Value.Should().Be(7);
    }

    [Fact]
    public async Task ExecuteAsync_RenewLeaseFromDoWork_ExtendsLease()
    {
        var fixture = CreateFixture();
        var lrt = fixture.CreateLrt();
        var previousLeaseExpiresAt = fixture.Job.LeaseExpiresAt!.Value;
        lrt.Work = async x =>
        {
            await x.RenewLeaseForTest(TimeSpan.FromMinutes(10));
            x.CapturedLeaseExpiresAt = x.CurrentLeaseExpiresAt;
        };

        await lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        lrt.CapturedLeaseExpiresAt.Should().BeAfter(previousLeaseExpiresAt);
    }

    [Fact]
    public async Task ExecuteAsync_MissingJob_Throws()
    {
        var fixture = CreateFixture();
        fixture.JobRepository
            .Setup(x => x.GetById(fixture.JobId, It.IsAny<CancellationToken>()))
            .Returns(() => new ValueTask<Job?>((Job?)null));
        var lrt = fixture.CreateLrt();

        var act = () => lrt.ExecuteAsync(fixture.JobId, fixture.LeaseHolderId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_InitializesExecutionContext()
    {
        var fixture = CreateFixture();
        var lrt = fixture.CreateLrt();
        using var cancellationTokenSource = new CancellationTokenSource();
        lrt.Work = x =>
        {
            x.CapturedJobId = x.CurrentJobId;
            x.CapturedLeaseHolderId = x.CurrentLeaseHolderId;
            x.CapturedCancellationToken = x.CurrentCancellationToken;
            x.CapturedInitialized = x.CurrentInitialized;
            return Task.CompletedTask;
        };

        await lrt.ExecuteAsync(
            fixture.JobId,
            fixture.LeaseHolderId,
            cancellationTokenSource.Token);

        lrt.CapturedJobId.Should().Be(fixture.JobId);
        lrt.CapturedLeaseHolderId.Should().Be(fixture.LeaseHolderId);
        lrt.CapturedCancellationToken.Should().Be(cancellationTokenSource.Token);
        lrt.CapturedInitialized.Should().BeTrue();
    }

    private static TestFixture CreateFixture(
        string initialState = "{}",
        int maxAttempts = 3)
    {
        return new TestFixture(initialState, maxAttempts);
    }

    private static void SetJobId(Job job, Guid id)
    {
        typeof(Job)
            .GetProperty(nameof(Job.Id))!
            .SetValue(job, id);
    }

    private sealed class TestFixture
    {
        public TestFixture(
            string initialState,
            int maxAttempts)
        {
            JobId = Guid.NewGuid();
            LeaseHolderId = Guid.NewGuid();
            Job = Job.Create("test-lrt", initialState, maxAttempts);
            SetJobId(Job, JobId);
            Job.AcquireLease(LeaseHolderId, TimeSpan.FromMinutes(5));

            JobRepository
                .Setup(x => x.GetById(JobId, It.IsAny<CancellationToken>()))
                .Returns(() => new ValueTask<Job?>(Job));

            UnitOfWork
                .SetupGet(x => x.Context)
                .Returns(new UnitOfWorkContext());

            UnitOfWork
                .Setup(x => x.ExecuteWithTransaction(
                    It.IsAny<TransactionalAttribute>(),
                    It.IsAny<Func<Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns<TransactionalAttribute, Func<Task>, CancellationToken>((_, action, _) => action());

            UnitOfWork
                .Setup(x => x.ExecuteWithTransaction(
                    It.IsAny<TransactionalAttribute>(),
                    It.IsAny<Func<Task<bool>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns<TransactionalAttribute, Func<Task<bool>>, CancellationToken>((_, action, _) => action());

            UnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            UnitOfWork
                .Setup(x => x.ReloadAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        public Guid JobId { get; }
        public Guid LeaseHolderId { get; }
        public Job Job { get; }
        public Mock<IRepository<Job, Guid>> JobRepository { get; } = new();
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public MessageBrokerStub Publisher { get; } = new();
        public IReadOnlyList<JobStatusUpdatedEvent> JobStatusEvents =>
            Publisher.PublishedMessagesOfType<JobStatusUpdatedEvent>();
        public Mock<ILogger> Logger { get; } = new();

        public TestLrt CreateLrt()
        {
            return new TestLrt(
                JobRepository.Object,
                UnitOfWork.Object,
                Publisher,
                Logger.Object);
        }
    }

    private sealed class TestLrt(
        IRepository<Job, Guid> jobRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publisher,
        ILogger logger
    ) : LrtBase(jobRepository, unitOfWork, publisher, logger)
    {
        public Func<TestLrt, Task> Work { get; set; } = _ => Task.CompletedTask;
        public int DoWorkCalls { get; private set; }
        public TestState? CapturedState { get; private set; }
        public DateTime? CapturedLeaseExpiresAt { get; set; }
        public Guid CapturedJobId { get; set; }
        public Guid CapturedLeaseHolderId { get; set; }
        public CancellationToken CapturedCancellationToken { get; set; }
        public bool CapturedInitialized { get; set; }
        public Guid CurrentJobId => JobId;
        public Guid CurrentLeaseHolderId => LeaseHolderId;
        public CancellationToken CurrentCancellationToken => CancellationToken;
        public bool CurrentInitialized => Initialized;
        public DateTime? CurrentLeaseExpiresAt => Job.LeaseExpiresAt;
        public override IServiceDefinition ServiceDefinition { get; } = new TestServiceDefinition();
        public override Type InputType => typeof(TestInput);
        public override Type StateType => typeof(TestState);

        protected override Task DoWork()
        {
            DoWorkCalls++;
            return Work(this);
        }

        public void InterruptForTest(string reason)
        {
            Interrupt(reason);
        }

        public void RequestCancellationForTest(string reason)
        {
            Job.RequestCancellation(reason);
        }

        public async Task CaptureStateForTest()
        {
            CapturedState = await GetStateAsync<TestState>();
        }

        public Task UpdateStateForTest(TestState state)
        {
            return UpdateState(state);
        }

        public Task RenewLeaseForTest(TimeSpan leaseDuration)
        {
            return RenewLeaseAsync(leaseDuration);
        }
    }

    private sealed class TestServiceDefinition : IServiceDefinition
    {
        public string ServiceName => "test-service";
    }

    private sealed class TestInput;

    private sealed class TestState
    {
        public int Value { get; set; }
    }
}
