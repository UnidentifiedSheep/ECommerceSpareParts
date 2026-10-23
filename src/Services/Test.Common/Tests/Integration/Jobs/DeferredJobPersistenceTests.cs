using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NamedObject.Core.Interfaces;
using Tests.Integration;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Jobs;

public sealed class DeferredJobPersistenceTests(CombinedContainerFixture fixture) : CommonLayerIntegrationTest(fixture)
{
	[Fact]
	public async Task SaveChanges_DeferredStepHasNoPreparedInputOrOutput()
	{
		var (step, _) = await CreateLeasedStepAsync();
		Context.ChangeTracker.Clear();

		var saved = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == step.Id, CancellationToken);

		saved.InputInitialized.Should().BeFalse();
		saved.StepKey.Should().Be("import");
		saved.Output.Should().BeNull();
		saved.State.Should().Be("{}");
	}

	[Fact]
	public async Task ExecuteAsync_CommitPersistsInitializedInputAndOutputWithSucceededStatus()
	{
		var (step, lease) = await CreateLeasedStepAsync();
		await using var transaction = Sp.GetRequiredService<IApplicationTransactionFactory>().Create();

		await transaction.WithSettings(TransactionalAttribute.ReadCommitted(30, 1)).ExecuteAsync(async (context, ct) =>
		{
			var saved = await context.Repositories.Get<Job, Guid>().GetById(step.Id, ct) ??
				throw new InvalidOperationException();
			saved.InitializeInput(LrtStateSerializer.SerializeInput("{}"), lease);
			saved.Start(lease);
			saved.Succeed(lease, "{\"StorageKey\":\"files/result.csv\"}");
			await context.UnitOfWork.SaveChangesAsync(ct);
		}, CancellationToken);

		Context.ChangeTracker.Clear();
		var committed = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == step.Id, CancellationToken);
		committed.InputInitialized.Should().BeTrue();
		committed.Status.Should().Be(JobStatus.Succeeded);
		committed.Output.Should().Be("{\"StorageKey\":\"files/result.csv\"}");
		LrtStateSerializer.DeserializeInput<NoneInputState>(committed.State).Should().NotBeNull();
	}

	[Fact]
	public async Task ExecuteAsync_RollbackDoesNotExposePreparedInputOrOutput()
	{
		var (step, lease) = await CreateLeasedStepAsync();
		await using var transaction = Sp.GetRequiredService<IApplicationTransactionFactory>().Create();
		var action = () => transaction.WithSettings(TransactionalAttribute.ReadCommitted(30, 1)).ExecuteAsync(async (context, ct) =>
		{
			var saved = await context.Repositories.Get<Job, Guid>().GetById(step.Id, ct) ??
				throw new InvalidOperationException();
			saved.InitializeInput(LrtStateSerializer.SerializeInput("{}"), lease);
			saved.Start(lease);
			saved.Succeed(lease, "{\"StorageKey\":\"files/result.csv\"}");
			await context.UnitOfWork.SaveChangesAsync(ct);
			throw new InvalidOperationException("rollback after save");
		}, CancellationToken);

		await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("rollback after save");

		Context.ChangeTracker.Clear();
		var rolledBack = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == step.Id, CancellationToken);
		rolledBack.InputInitialized.Should().BeFalse();
		rolledBack.Status.Should().Be(JobStatus.Locked);
		rolledBack.Output.Should().BeNull();
		rolledBack.State.Should().Be("{}");
	}

	[Fact]
	public async Task SaveChanges_RootJobInputIsInitialized()
	{
		var job = SingleRunJob.Create("root", "{}");
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var saved = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == job.Id, CancellationToken);

		saved.InputInitialized.Should().BeTrue();
		saved.State.Should().Be("{}");
		saved.StepKey.Should().BeNull();
		saved.Output.Should().BeNull();
	}

	[Fact]
	public async Task ConcurrentInitialization_PreparesInputOnceFromPersistedDependency()
	{
		var preparations = 0;
		var workflow = new Mock<IMultiStepLrt>();
		workflow.SetupGet(x => x.SystemName).Returns("workflow");
		workflow.SetupGet(x => x.InputType).Returns(typeof(NoneInputState));
		workflow.Setup(x => x.ConfigureSteps(It.IsAny<IMultiStepJobBuilder>())).Callback<IMultiStepJobBuilder>(builder =>
		{
			var producer = builder.AddStep("producer", new NoneInputState(), key: "download");
			var consumer = builder.AddStep("consumer", context =>
			{
				Interlocked.Increment(ref preparations);
				return new FileInput(context.GetOutput<FileOutput>(producer).StorageKey);
			}, key: "import");
			builder.AddDependency(consumer, producer);
		});
		var producerLrt = new Mock<ILrtNamedObject>();
		producerLrt.SetupGet(x => x.SystemName).Returns("producer");
		producerLrt.SetupGet(x => x.InputType).Returns(typeof(NoneInputState));
		producerLrt.SetupGet(x => x.OutputType).Returns(typeof(FileOutput));
		var consumerLrt = new Mock<ILrtNamedObject>();
		consumerLrt.SetupGet(x => x.SystemName).Returns("consumer");
		consumerLrt.SetupGet(x => x.InputType).Returns(typeof(FileInput));
		var registry = new Mock<INamedObjectRegistry<ILrtNamedObject>>();
		registry.Setup(x => x.GetBySystemName(It.IsAny<string>())).Returns((string name) => name switch
		{
			"workflow" => workflow.Object,
			"producer" => producerLrt.Object,
			"consumer" => consumerLrt.Object,
			_ => throw new InvalidOperationException(name)
		});
		var parent = (MultiStepJob)new JobCreationDispatcher(registry.Object).Create("workflow", "{}", 3);
		var producer = parent.Steps[0];
		var consumer = parent.Steps[1];
		var parentLease = Guid.NewGuid();
		parent.AcquireLease(parentLease, TimeSpan.FromMinutes(5));
		parent.Start(parentLease);
		parent.ActivateStep(producer);
		var producerLease = Guid.NewGuid();
		producer.AcquireLease(producerLease, TimeSpan.FromMinutes(5));
		producer.InitializeInput(LrtStateSerializer.SerializeInput("{}"), producerLease);
		producer.Start(producerLease);
		producer.Succeed(producerLease, JsonSerializer.Serialize(new FileOutput("files/persisted.csv")));
		parent.ActivateStep(consumer);
		parent.Wait(parentLease);
		var lease = Guid.NewGuid();
		consumer.AcquireLease(lease, TimeSpan.FromMinutes(5));
		await Context.AddAsync(parent, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		preparations.Should().Be(0);

		var initializer = new JobStepInputInitializer(registry.Object);
		async Task PrepareInputAsync()
		{
			await using var transaction = Sp.GetRequiredService<IApplicationTransactionFactory>().Create();
			await transaction.WithSettings(TransactionalAttribute.ReadCommitted(30, 1)).ExecuteAsync(async (context, ct) =>
			{
				await initializer.InitializeAsync(consumer, lease, context, ct);
				await Task.Yield();
				await context.UnitOfWork.SaveChangesAsync(ct);
			}, CancellationToken);
		}

		await Task.WhenAll(PrepareInputAsync(), PrepareInputAsync());

		Context.ChangeTracker.Clear();
		var saved = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == consumer.Id, CancellationToken);
		saved.InputInitialized.Should().BeTrue();
		LrtStateSerializer.DeserializeInput<FileInput>(saved.State).StorageKey.Should().Be("files/persisted.csv");
		preparations.Should().Be(1);
	}

	private sealed record FileOutput(string StorageKey);
	private sealed record FileInput(string StorageKey) : IInputState
	{
		public void ValidateState() => ArgumentException.ThrowIfNullOrWhiteSpace(StorageKey);
	}

	private async Task<(Job Step, Guid Lease)> CreateLeasedStepAsync()
	{
		var parent = MultiStepJob.Create("workflow", "{}");
		var step = SingleRunJob.CreateDeferred("step");
		parent.AddStep(step, "import");
		var parentLease = Guid.NewGuid();
		parent.AcquireLease(parentLease, TimeSpan.FromMinutes(5));
		parent.Start(parentLease);
		parent.ActivateStep(step);
		parent.Wait(parentLease);
		var lease = Guid.NewGuid();
		step.AcquireLease(lease, TimeSpan.FromMinutes(5));
		await Context.AddAsync(parent, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		return (step, lease);
	}
}
