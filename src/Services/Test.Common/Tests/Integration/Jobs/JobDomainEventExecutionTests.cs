using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Integration;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Jobs;

public sealed class JobDomainEventExecutionTests(CombinedContainerFixture fixture)
	: CommonLayerIntegrationTest(fixture)
{
	[Fact]
	public async Task StepSucceeds_ExecutorResumesWaitingParent()
	{
		var parentLeaseHolderId = Guid.NewGuid();
		var parent = MultiStepJob.Create("parent", "{}");
		var step = SingleRunJob.Create("step", "{}");

		parent.AddStep(step);
		parent.AcquireLease(parentLeaseHolderId, TimeSpan.FromMinutes(5));
		parent.Start(parentLeaseHolderId);
		parent.ActivateStep(step);
		parent.Wait(parentLeaseHolderId);

		await Context.AddAsync(parent, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var leaseHolderId = Guid.NewGuid();
		var leaseService = Scope.ServiceProvider.GetRequiredService<IJobLeaseService>();
		var leasedStep = await leaseService.TryAcquireJobAsync(
			leaseHolderId,
			TimeSpan.FromMinutes(5),
			CancellationToken.None);

		leasedStep!.Id.Should().Be(step.Id);

		var repository = Scope.ServiceProvider.GetRequiredService<IRepository<Job, Guid>>();
		var unitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
		var executor = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>();
		var lrt = new SucceedingTestLrt(
			executor,
			Mock.Of<ILogger>());

		await lrt.ExecuteAsync(
			step.Id,
			leaseHolderId,
			CancellationToken);

		Context.ChangeTracker.Clear();
		var persistedParent =
			await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == parent.Id, CancellationToken);

		persistedParent.Status.Should().Be(JobStatus.Pending);
	}

	[Fact]
	public async Task StepLeaseExpiresWithoutAttempts_ResumesWaitingParent()
	{
		var parentLeaseHolderId = Guid.NewGuid();
		var stepLeaseHolderId = Guid.NewGuid();
		var parent = MultiStepJob.Create("parent", "{}");
		var step = SingleRunJob.Create(
			"step",
			"{}",
			1);

		parent.AddStep(step);
		parent.AcquireLease(parentLeaseHolderId, TimeSpan.FromMinutes(5));
		parent.Start(parentLeaseHolderId);
		parent.ActivateStep(step);
		parent.Wait(parentLeaseHolderId);

		step.AcquireLease(stepLeaseHolderId, TimeSpan.FromSeconds(-1));

		await Context.AddAsync(parent, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var leaseService = Scope.ServiceProvider.GetRequiredService<IJobLeaseService>();

		var failedJobs = await leaseService.FailExpiredJobsWithoutAttempts(10, CancellationToken.None);

		failedJobs.Should().ContainSingle(x => x.Id == step.Id);

		Context.ChangeTracker.Clear();
		var jobs = await Context
			.Jobs
			.AsNoTracking()
			.Where(x => x.Id == parent.Id || x.Id == step.Id)
			.ToDictionaryAsync(x => x.Id, CancellationToken);

		jobs[parent.Id].Status.Should().Be(JobStatus.Pending);
		jobs[step.Id].Status.Should().Be(JobStatus.Failed);
	}

	[Fact]
	public async Task CancellationRequestedJobWithExpiredLease_IsCancelled()
	{
		var job = SingleRunJob.Create("job", "{}", 3);
		job.AcquireLease(Guid.NewGuid(), TimeSpan.FromSeconds(-1));
		job.RequestCancellation("stop requested");
		var activeJob = SingleRunJob.Create("active-job", "{}", 3);
		activeJob.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));
		activeJob.RequestCancellation();

		await Context.AddAsync(job, CancellationToken);
		await Context.AddAsync(activeJob, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var leaseService = Scope.ServiceProvider.GetRequiredService<IJobLeaseService>();
		var cancelledJobs = await leaseService.CancelExpiredJobs(10, CancellationToken.None);

		cancelledJobs.Should().ContainSingle(x => x.Id == job.Id);
		Context.ChangeTracker.Clear();
		var persisted = await Context.Jobs.AsNoTracking()
			.SingleAsync(x => x.Id == job.Id, CancellationToken);
		persisted.Status.Should().Be(JobStatus.Cancelled);
		persisted.ErrorMessage.Should().Be("stop requested");
		persisted.LeaseHolderId.Should().BeNull();
		persisted.LeaseExpiresAt.Should().BeNull();
		(await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == activeJob.Id, CancellationToken))
			.Status.Should().Be(JobStatus.CancellationRequested);
	}

	private sealed class SucceedingTestLrt(IApplicationTransactionFactory transactionService, ILogger logger) : LrtBase<NoneInputState, NoneInputState>(
		transactionService,
		logger)
	{
		public override string SystemName => "step";

		public override ILocalizableMessage NameLocalizationMessage => new LocalizableMessage("test-name");

		public override ILocalizableMessage DescriptionLocalizationMessage =>
			new LocalizableMessage("test-description");

		protected override Task DoWork(NoneInputState inputState) => Task.CompletedTask;
	}
}
