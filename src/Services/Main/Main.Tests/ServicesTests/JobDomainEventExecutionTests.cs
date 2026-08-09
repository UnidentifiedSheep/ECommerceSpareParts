using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.ServicesTests; //TODO should be moved to Test.Common.

public sealed class JobDomainEventExecutionTests(
    CombinedContainerFixture fixture)
    : IntegrationTest(fixture)
{
    [Fact]
    public async Task StepSucceeds_ExecutorResumesWaitingParent()
    {
        var parentLeaseHolderId = Guid.NewGuid();
        var parent = MultiStepJob.Create("parent", "{}");
        var step = SingleRunJob.Create("step", "{}");

        parent.AddStep(step);
        parent.AcquireLease(
            parentLeaseHolderId,
            TimeSpan.FromMinutes(5));
        parent.Start(parentLeaseHolderId);
        parent.ActivateStep(step);
        parent.Wait(parentLeaseHolderId);

        await Context.AddAsync(parent);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var leaseHolderId = Guid.NewGuid();
        var leaseService = Scope.ServiceProvider
            .GetRequiredService<IJobLeaseService>();
        var leasedStep = await leaseService.TryAcquireJobAsync(
            leaseHolderId,
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        leasedStep!.Id.Should().Be(step.Id);

        var repository = Scope.ServiceProvider
            .GetRequiredService<IRepository<Job, Guid>>();
        var unitOfWork = Scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();
        var executor = Scope.ServiceProvider
            .GetRequiredService<IDomainEventExecutor>();
        var lrt = new SucceedingTestLrt(
            repository,
            unitOfWork,
            new MessageBrokerStub(),
            executor,
            Mock.Of<ILogger>());

        await lrt.ExecuteAsync(step.Id, leaseHolderId);

        Context.ChangeTracker.Clear();
        var persistedParent = await Context.Jobs
            .AsNoTracking()
            .SingleAsync(x => x.Id == parent.Id);

        persistedParent.Status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public async Task StepLeaseExpiresWithoutAttempts_ResumesWaitingParent()
    {
        var parentLeaseHolderId = Guid.NewGuid();
        var stepLeaseHolderId = Guid.NewGuid();
        var parent = MultiStepJob.Create("parent", "{}");
        var step = SingleRunJob.Create("step", "{}", maxAttempts: 1);

        parent.AddStep(step);
        parent.AcquireLease(
            parentLeaseHolderId,
            TimeSpan.FromMinutes(5));
        parent.Start(parentLeaseHolderId);
        parent.ActivateStep(step);
        parent.Wait(parentLeaseHolderId);

        step.AcquireLease(
            stepLeaseHolderId,
            TimeSpan.FromSeconds(-1));

        await Context.AddAsync(parent);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var leaseService = Scope.ServiceProvider
            .GetRequiredService<IJobLeaseService>();

        var failedJobs = await leaseService
            .FailExpiredJobsWithoutAttempts(10, CancellationToken.None);

        failedJobs.Should().ContainSingle(x => x.Id == step.Id);

        Context.ChangeTracker.Clear();
        var jobs = await Context.Jobs
            .AsNoTracking()
            .Where(x => x.Id == parent.Id || x.Id == step.Id)
            .ToDictionaryAsync(x => x.Id);

        jobs[parent.Id].Status.Should().Be(JobStatus.Pending);
        jobs[step.Id].Status.Should().Be(JobStatus.Failed);
    }

    private sealed class SucceedingTestLrt(
        IRepository<Job, Guid> jobRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publisher,
        IDomainEventExecutor domainEventExecutor,
        ILogger logger)
        : LrtBase<NoneInputState, NoneInputState>(
            jobRepository,
            unitOfWork,
            publisher,
            domainEventExecutor,
            logger)
    {
        public override IServiceDefinition ServiceDefinition { get; } =
            new TestServiceDefinition();
        public override string SystemName => "step";
        public override string NameLocalizationKey => "test-name";
        public override string DescriptionLocalizationKey =>
            "test-description";

        protected override Task DoWork() => Task.CompletedTask;
    }

    private sealed class TestServiceDefinition : IServiceDefinition
    {
        public string ServiceName => "test";
    }
}
