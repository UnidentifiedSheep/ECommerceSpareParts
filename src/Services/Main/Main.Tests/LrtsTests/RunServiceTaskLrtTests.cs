using System.Text.Json;
using Application.Common.Extensions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Contracts.Tasks;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.RunServiceTask;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Stubs;

namespace Tests.LrtsTests;

public sealed class RunServiceTaskLrtTests
{
	[Theory]
	[InlineData("search")]
	[InlineData("another-service")]
	public async Task Execute_PublishesTheTaskNameWithTheServiceAsRoutingKey(string serviceName)
	{
		var events = new IntegrationEventScope();
		var repositories = new Mock<IRepository<Job, Guid>>();
		var unitOfWork = new Mock<IUnitOfWork>();
		unitOfWork.Setup(value => value.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		unitOfWork.Setup(value => value.ReloadAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		var lrt = new RunServiceTaskLrt(repositories.Object, unitOfWork.Object, Mock.Of<IPublishEndpoint>(),
			new ApplicationTransactionServiceStub(unitOfWork.Object, Mock.Of<IRepositoryProvider>(), events),
			events, NullLogger<RunServiceTaskLrt>.Instance);
		var input = new RunServiceTaskInputState { ServiceName = serviceName, TaskSystemName = "CleanupProductIndex" };
		var job = SingleRunJob.Create(lrt.SystemName, lrt.ValidateState(JsonSerializer.Serialize(input)), 1);
		var leaseHolderId = Guid.NewGuid();
		job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
		repositories.Setup(value => value.GetById(job.Id, It.IsAny<CancellationToken>()))
			.Returns(() => new ValueTask<Job?>(job));

		await lrt.ExecuteAsync(job.Id, leaseHolderId, TestContext.Current.CancellationToken);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var envelope = events.Flush().Should().ContainSingle().Which;
		envelope.RoutingKey.Should().Be(serviceName);
		envelope.Message.Should().BeEquivalentTo(new ServiceTaskRequestedEvent { TaskSystemName = input.TaskSystemName });
	}

	[Theory]
	[InlineData("", "cleanup")]
	[InlineData("  ", "cleanup")]
	[InlineData("search", "")]
	[InlineData("search", "  ")]
	public void ValidateState_RejectsMissingServiceOrTaskName(string serviceName, string taskName)
	{
		var state = new RunServiceTaskInputState { ServiceName = serviceName, TaskSystemName = taskName };
		state.Invoking(value => value.ValidateState()).Should().Throw<InvalidOperationException>();
	}
}
