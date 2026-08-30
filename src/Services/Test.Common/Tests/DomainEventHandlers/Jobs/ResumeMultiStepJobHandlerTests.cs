using Application.Common.DomainEventHandlers.Jobs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Domain.CommonEntities.Job;
using Domain.CommonEntities.Job.Events;
using Domain.CommonEnums;
using FluentAssertions;
using Moq;

namespace Tests.Tests.DomainEventHandlers.Jobs;

public sealed class ResumeMultiStepJobHandlerTests
{
	[Fact]
	public async Task Handle_WaitingParent_ResumesIt()
	{
		var parent = CreateWaitingParent();
		var repository = CreateRepository(parent);
		var handler = new ResumeMultiStepJobHandler(repository.Object);

		await handler.Handle(CreateBatch(parent.Id), CancellationToken.None);

		parent.Status.Should().Be(JobStatus.Pending);
	}

	[Fact]
	public async Task Handle_ParentIsNotWaiting_DoesNothing()
	{
		var parent = MultiStepJob.Create("multi-step", "{}");
		var repository = CreateRepository(parent);
		var handler = new ResumeMultiStepJobHandler(repository.Object);

		await handler.Handle(CreateBatch(parent.Id), CancellationToken.None);

		parent.Status.Should().Be(JobStatus.Pending);
	}

	[Fact]
	public async Task Handle_MultipleParents_ResumesWaitingParentsWithOneQuery()
	{
		var firstParent = CreateWaitingParent();
		var secondParent = CreateWaitingParent();
		var repository = CreateRepository(firstParent, secondParent);
		var handler = new ResumeMultiStepJobHandler(repository.Object);
		var batch = new Batch<JobStepFinishedDomainEvent>(
			[CreateEvent(firstParent.Id), CreateEvent(firstParent.Id), CreateEvent(secondParent.Id)]);

		await handler.Handle(batch, CancellationToken.None);

		firstParent.Status.Should().Be(JobStatus.Pending);
		secondParent.Status.Should().Be(JobStatus.Pending);
		repository.Verify(
			x => x.ListAsync(It.IsAny<Criteria<Job>>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	private static Batch<JobStepFinishedDomainEvent> CreateBatch(Guid parentId) =>
		new([CreateEvent(parentId)]);

	private static JobStepFinishedDomainEvent CreateEvent(Guid parentId) => new(
		Guid.NewGuid(),
		parentId,
		JobStatus.Succeeded);

	private static Mock<IRepository<Job, Guid>> CreateRepository(params Job[] jobs)
	{
		var repository = new Mock<IRepository<Job, Guid>>();
		repository
			.Setup(x => x.ListAsync(It.IsAny<Criteria<Job>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(jobs.ToList());
		return repository;
	}

	private static MultiStepJob CreateWaitingParent()
	{
		var leaseHolderId = Guid.NewGuid();
		var parent = MultiStepJob.Create("multi-step", "{}");
		parent.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
		parent.Start(leaseHolderId);
		parent.Wait(leaseHolderId);
		return parent;
	}
}
