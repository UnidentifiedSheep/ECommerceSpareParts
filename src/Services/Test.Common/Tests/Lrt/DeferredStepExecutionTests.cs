using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.Models.Lrt;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NamedObject.Core.Interfaces;
using Persistence;
using Tests.Stubs;

namespace Tests.Tests.Lrt;

public sealed class DeferredStepExecutionTests
{
	[Fact]
	public void CreateJob_DoesNotRunInputFactory()
	{
		using var fixture = new Fixture();
		fixture.Preparations.Should().Be(0);
		fixture.Consumer.InputInitialized.Should().BeFalse();
		fixture.Consumer.Status.Should().Be(JobStatus.Blocked);
		fixture.Consumer.StepKey.Should().Be("import");
	}

	[Fact]
	public async Task ExecuteAsync_ReadsTypedOutputAndParentInputAfterDependencySucceeds()
	{
		using var fixture = new Fixture();
		await fixture.ReadyConsumerAsync();
		fixture.Preparations.Should().Be(0);
		fixture.Producer.Output.Should().NotBeNull();

		var lrt = fixture.CreateConsumer();
		await fixture.ExecuteAsync(lrt, fixture.Consumer, TestContext.Current.CancellationToken);

		fixture.Preparations.Should().Be(1);
		fixture.Consumer.Status.Should().Be(JobStatus.Succeeded);
		fixture.Consumer.InputInitialized.Should().BeTrue();
		lrt.ObservedInput.Should().BeEquivalentTo(new StepInput("files/source.csv", 42));
		lrt.ObservedProgress.Should().Be(7);
		LrtStateSerializer.DeserializeInput<StepInput>(fixture.Consumer.State).Should().BeEquivalentTo(lrt.ObservedInput);
	}

	[Fact]
	public async Task ExecuteAsync_RetryUsesSavedInputWithoutCallingFactoryAgain()
	{
		using var fixture = new Fixture();
		await fixture.ReadyConsumerAsync();
		var lrt = fixture.CreateConsumer();
		lrt.Work = _ => lrt.Calls == 1 ? throw new InvalidOperationException("retry") : Task.CompletedTask;

		await fixture.ExecuteAsync(lrt, fixture.Consumer, TestContext.Current.CancellationToken);

		fixture.Consumer.Status.Should().Be(JobStatus.Succeeded);
		fixture.Consumer.Attempts.Should().Be(2);
		fixture.Preparations.Should().Be(1);
		lrt.Calls.Should().Be(2);
	}

	[Fact]
	public async Task ExecuteAsync_NewLrtInstanceResumesSavedInputAndProgress()
	{
		using var fixture = new Fixture();
		await fixture.ReadyConsumerAsync();
		using var stopped = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
		var first = fixture.CreateConsumer();
		first.Work = async lrt =>
		{
			await lrt.CheckpointAsync(27);
			await lrt.ExpireLeaseAsync();
			await stopped.CancelAsync();
			throw new OperationCanceledException(stopped.Token);
		};
		await fixture.ExecuteAsync(first, fixture.Consumer, stopped.Token);
		fixture.Consumer.Status.Should().Be(JobStatus.Processing);
		fixture.RejectPreparation = true;

		var restarted = fixture.CreateConsumer();
		await fixture.ExecuteAsync(restarted, fixture.Consumer, TestContext.Current.CancellationToken);

		fixture.Consumer.Status.Should().Be(JobStatus.Succeeded);
		fixture.Consumer.Attempts.Should().Be(2);
		fixture.Preparations.Should().Be(1);
		restarted.ObservedProgress.Should().Be(27);
		restarted.ObservedInput!.StorageKey.Should().Be("files/source.csv");
	}

	[Fact]
	public async Task ExecuteAsync_FactoryFailureIsRetriedBeforeWorkStarts()
	{
		using var fixture = new Fixture { PreparationFailures = 1 };
		await fixture.ReadyConsumerAsync();
		var lrt = fixture.CreateConsumer();

		await fixture.ExecuteAsync(lrt, fixture.Consumer, TestContext.Current.CancellationToken);

		fixture.Consumer.Status.Should().Be(JobStatus.Succeeded);
		fixture.Consumer.Attempts.Should().Be(2);
		fixture.Preparations.Should().Be(2);
		lrt.Calls.Should().Be(1);
	}

	[Fact]
	public async Task ExecuteAsync_InvalidPreparedInputFailsWithoutExecutingWork()
	{
		using var fixture = new Fixture { InvalidInput = true };
		await fixture.ReadyConsumerAsync();
		var lrt = fixture.CreateConsumer();

		await fixture.ExecuteAsync(lrt, fixture.Consumer, TestContext.Current.CancellationToken);

		fixture.Consumer.Status.Should().Be(JobStatus.Failed);
		fixture.Consumer.InputInitialized.Should().BeFalse();
		fixture.Consumer.Output.Should().BeNull();
		lrt.Calls.Should().Be(0);
		await fixture.ResumeWorkflowAsync();
		fixture.Parent.Status.Should().Be(JobStatus.Failed);
	}

	[Fact]
	public async Task ExecuteAsync_UnfinishedDependencyCannotPrepareInput()
	{
		using var fixture = new Fixture();
		await fixture.StartWorkflowAsync();
		fixture.Parent.ActivateStep(fixture.Consumer);
		var lrt = fixture.CreateConsumer();

		await fixture.ExecuteAsync(lrt, fixture.Consumer, TestContext.Current.CancellationToken);

		fixture.Preparations.Should().Be(0);
		fixture.Consumer.InputInitialized.Should().BeFalse();
		fixture.Consumer.Status.Should().Be(JobStatus.Failed);
		lrt.Calls.Should().Be(0);
	}

	[Fact]
	public async Task ExecuteAsync_CancelledParentCancelsStepWithoutPreparingInput()
	{
		using var fixture = new Fixture();
		await fixture.ReadyConsumerAsync();
		fixture.Parent.RequestCancellation("stop");
		var lrt = fixture.CreateConsumer();

		await fixture.ExecuteAsync(lrt, fixture.Consumer, TestContext.Current.CancellationToken);

		fixture.Preparations.Should().Be(0);
		fixture.Consumer.InputInitialized.Should().BeFalse();
		fixture.Consumer.Status.Should().Be(JobStatus.Cancelled);
		lrt.Calls.Should().Be(0);
	}

	[Fact]
	public async Task ExecuteAsync_FailedProducerDoesNotPublishOutputOrPrepareNextStep()
	{
		using var fixture = new Fixture();
		await fixture.StartWorkflowAsync();
		var lease = Guid.NewGuid();
		fixture.Producer.AcquireLease(lease, TimeSpan.FromMinutes(5));
		fixture.Producer.InitializeInput(LrtStateSerializer.SerializeInput("{}"), lease);
		fixture.Producer.Start(lease);
		fixture.Producer.Fail(lease, "download failed");

		await fixture.ResumeWorkflowAsync();

		fixture.Producer.Output.Should().BeNull();
		fixture.Preparations.Should().Be(0);
		fixture.Consumer.Status.Should().Be(JobStatus.Cancelled);
	}

	[Fact]
	public async Task ExecuteAsync_LostLeaseCannotPrepareInput()
	{
		using var fixture = new Fixture();
		await fixture.ReadyConsumerAsync();
		fixture.Consumer.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));
		var lrt = fixture.CreateConsumer();

		await lrt.ExecuteAsync(fixture.Consumer.Id, Guid.NewGuid(), TestContext.Current.CancellationToken);

		fixture.Preparations.Should().Be(0);
		fixture.Consumer.InputInitialized.Should().BeFalse();
		fixture.Consumer.Status.Should().Be(JobStatus.Locked);
		lrt.Calls.Should().Be(0);
	}

	[Fact]
	public async Task ExecuteAsync_WorkflowPublishesOutputOnlyAfterAllStepsSucceed()
	{
		using var fixture = new Fixture();
		await fixture.ReadyConsumerAsync();
		fixture.Parent.Output.Should().BeNull();
		await fixture.ExecuteAsync(fixture.CreateConsumer(), fixture.Consumer, TestContext.Current.CancellationToken);

		await fixture.ResumeWorkflowAsync();

		fixture.Parent.Status.Should().Be(JobStatus.Succeeded);
		fixture.Parent.Output.Should().Contain("files/workflow-output.csv");
	}

	[Fact]
	public async Task ExecuteAsync_NestedWorkflowPreparesInputWithRetryThenPassesItToItsOwnStep()
	{
		using var fixture = new Fixture(nested: true) { PreparationFailures = 1 };
		await fixture.ReadyConsumerAsync();
		var nested = (MultiStepJob)fixture.Consumer;
		nested.MaxAttempts.Should().Be(3);
		var leaf = nested.Steps.Single();

		await fixture.ExecuteAsync(fixture.CreateNested(), nested, TestContext.Current.CancellationToken);

		fixture.Preparations.Should().Be(2);
		nested.Attempts.Should().Be(2);
		nested.Status.Should().Be(JobStatus.Waiting);
		nested.Output.Should().BeNull();
		leaf.InputInitialized.Should().BeFalse();
		var consumer = fixture.CreateConsumer();
		await fixture.ExecuteAsync(consumer, leaf, TestContext.Current.CancellationToken);
		consumer.ObservedInput.Should().BeEquivalentTo(new StepInput("files/source.csv", 42));
		nested.Resume();
		await fixture.ExecuteAsync(fixture.CreateNested(), nested, TestContext.Current.CancellationToken);
		await fixture.ResumeWorkflowAsync();

		nested.Status.Should().Be(JobStatus.Succeeded);
		nested.Output.Should().Contain("files/source.csv");
		fixture.Parent.Status.Should().Be(JobStatus.Succeeded);
	}

	private sealed class Fixture : IDisposable
	{
		private readonly ServiceProvider _services;
		private readonly bool _nested;
		private readonly Dictionary<Guid, Job> _jobs;
		private readonly IApplicationTransactionFactory _transactions;

		public Fixture(bool nested = false)
		{
			_nested = nested;
			var registry = new Mock<INamedObjectRegistry<ILrtNamedObject>>();
			_services = new ServiceCollection()
				.AddSingleton<IJobStepInputInitializer>(new JobStepInputInitializer(registry.Object))
				.BuildServiceProvider();
			var repository = new Mock<IRepository<Job, Guid>>();
			var unitOfWork = new Mock<IUnitOfWork>();
			unitOfWork.SetupGet(x => x.Context).Returns(new UnitOfWorkContext());
			unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			unitOfWork.Setup(x => x.ReloadAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			_transactions = new ApplicationTransactionFactoryStub(unitOfWork.Object,
				Mock.Of<IRepositoryProvider>(p => p.Get<Job, Guid>() == repository.Object),
				Mock.Of<IIntegrationEventScope>(), _services);
			registry.Setup(x => x.GetBySystemName(It.IsAny<string>())).Returns((string name) => name switch
			{
				"workflow" => CreateWorkflow(),
				"producer" => new ProducerLrt(_transactions),
				"consumer" => CreateConsumer(),
				"nested" => CreateNested(),
				_ => throw new InvalidOperationException(name)
			});
			Parent = (MultiStepJob)new JobCreationDispatcher(registry.Object).Create("workflow", "{\"BatchSize\":42}", 3);
			Producer = Parent.Steps.Single(x => x.StepKey == "download");
			Consumer = Parent.Steps.Single(x => x.StepKey == "import");
			_jobs = AllJobs(Parent).ToDictionary(x => x.Id);
			repository.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
				.Returns((Guid id, CancellationToken _) => new ValueTask<Job?>(_jobs[id]));
			repository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Criteria<Job>>(), It.IsAny<CancellationToken>()))
				.Returns((Criteria<Job>? criteria, CancellationToken _) => Task.FromResult(Filter(criteria).FirstOrDefault()));
			repository.Setup(x => x.ListAsync(It.IsAny<Criteria<Job>>(), It.IsAny<CancellationToken>()))
				.Returns((Criteria<Job>? criteria, CancellationToken _) => Task.FromResult(Filter(criteria).ToList()));
		}

		public MultiStepJob Parent { get; }
		public Job Producer { get; }
		public Job Consumer { get; }
		public int Preparations { get; private set; }
		public int PreparationFailures { get; init; }
		public bool RejectPreparation { get; set; }
		public bool InvalidInput { get; init; }

		private IEnumerable<Job> Filter(Criteria<Job>? criteria) => criteria?.Wheres.Aggregate(
			_jobs.Values.AsEnumerable(), (jobs, predicate) => jobs.Where(predicate.Compile())) ?? _jobs.Values;

		private WorkflowLrt CreateWorkflow() => new(_transactions, async (context, producer, ct) =>
		{
			ct.ThrowIfCancellationRequested();
			Preparations++;
			if (RejectPreparation || Preparations <= PreparationFailures)
				throw new InvalidOperationException("preparation failed");
			await Task.Yield();
			var output = context.GetOutput<FileOutput>(producer);
			return new StepInput(InvalidInput ? "" : output.StorageKey, context.GetParentInput<WorkflowInput>().BatchSize);
		}, _nested ? "nested" : "consumer");

		public ConsumerLrt CreateConsumer() => new(_transactions);
		public NestedWorkflowLrt CreateNested() => new(_transactions);

		private static IEnumerable<Job> AllJobs(Job job) => job is MultiStepJob workflow
			? workflow.Steps.SelectMany(AllJobs).Prepend(job) : [job];
		public Task StartWorkflowAsync() => ExecuteAsync(CreateWorkflow(), Parent, TestContext.Current.CancellationToken);

		public async Task ReadyConsumerAsync()
		{
			await StartWorkflowAsync();
			await ExecuteAsync(new ProducerLrt(_transactions), Producer, TestContext.Current.CancellationToken);
			await ResumeWorkflowAsync();
			Consumer.Status.Should().Be(JobStatus.Pending);
		}

		public Task ResumeWorkflowAsync()
		{
			Parent.Resume();
			return ExecuteAsync(CreateWorkflow(), Parent, TestContext.Current.CancellationToken);
		}

		public Task ExecuteAsync(ILrt lrt, Job job, CancellationToken cancellationToken)
		{
			var lease = Guid.NewGuid();
			job.AcquireLease(lease, TimeSpan.FromMinutes(5));
			return lrt.ExecuteAsync(job.Id, lease, cancellationToken);
		}

		public void Dispose() => _services.Dispose();
	}

	private sealed class WorkflowLrt(
		IApplicationTransactionFactory transactions,
		Func<LrtStepInputContext, LrtStepDefinition, CancellationToken, Task<StepInput>> prepare,
		string consumerSystemName)
		: MultiStepLrtBase<WorkflowInput, WorkflowInput, FileOutput>(transactions, Mock.Of<ILogger>())
	{
		public override string SystemName => "workflow";
		public override ILocalizableMessage NameLocalizationMessage => new LocalizableMessage("workflow");
		public override ILocalizableMessage DescriptionLocalizationMessage => NameLocalizationMessage;

		protected override Task<FileOutput> CreateOutputAsync() =>
			Task.FromResult(new FileOutput("files/workflow-output.csv"));

		protected override void ConfigureSteps(IMultiStepJobBuilder builder)
		{
			var producer = builder.AddStep("producer", new NoneInputState(), key: "download");
			var consumer = builder.AddStep(consumerSystemName, (context, ct) => prepare(context, producer, ct), key: "import");
			builder.AddDependency(consumer, producer);
		}
	}

	private sealed class NestedWorkflowLrt(IApplicationTransactionFactory transactions)
		: MultiStepLrtBase<StepInput, StepInput, FileOutput>(transactions, Mock.Of<ILogger>())
	{
		public override string SystemName => "nested";
		public override ILocalizableMessage NameLocalizationMessage => new LocalizableMessage("nested");
		public override ILocalizableMessage DescriptionLocalizationMessage => NameLocalizationMessage;
		protected override void ConfigureSteps(IMultiStepJobBuilder builder) =>
			builder.AddStep("consumer", context => context.GetParentInput<StepInput>(), key: "leaf");
		protected override Task<FileOutput> CreateOutputAsync() => Task.FromResult(new FileOutput(InputState.StorageKey));
	}

	private sealed class ProducerLrt(IApplicationTransactionFactory transactions)
		: LrtBase<NoneInputState, NoneInputState, FileOutput>(transactions, Mock.Of<ILogger>())
	{
		public override string SystemName => "producer";
		public override ILocalizableMessage NameLocalizationMessage => new LocalizableMessage("producer");
		public override ILocalizableMessage DescriptionLocalizationMessage => NameLocalizationMessage;
		protected override Task DoWork(NoneInputState inputState) => Task.CompletedTask;
		protected override Task<FileOutput> CreateOutputAsync() => Task.FromResult(new FileOutput("files/source.csv"));
	}

	private sealed class ConsumerLrt(IApplicationTransactionFactory transactions)
		: LrtBase<StepInput, Progress>(transactions, Mock.Of<ILogger>())
	{
		public override string SystemName => "consumer";
		public override ILocalizableMessage NameLocalizationMessage => new LocalizableMessage("consumer");
		public override ILocalizableMessage DescriptionLocalizationMessage => NameLocalizationMessage;
		public Func<ConsumerLrt, Task> Work { get; set; } = _ => Task.CompletedTask;
		public int Calls { get; private set; }
		public StepInput? ObservedInput { get; private set; }
		public int ObservedProgress { get; private set; }
		protected override Progress CreateInitialState(StepInput inputState) => new(7);
		protected override Task DoWork(StepInput inputState)
		{
			Calls++;
			ObservedInput = inputState;
			ObservedProgress = State.Rows;
			return Work(this);
		}
		public Task CheckpointAsync(int rows) => SaveStateAsync(new Progress(rows));
		public Task ExpireLeaseAsync() => RenewLeaseAsync(TimeSpan.FromSeconds(-1));
	}

	private sealed record FileOutput(string StorageKey);
	private sealed record Progress(int Rows);
	private sealed record WorkflowInput(int BatchSize) : IInputState
	{
		public void ValidateState() { }
	}
	private sealed record StepInput(string StorageKey, int BatchSize) : IInputState
	{
		public void ValidateState() => ArgumentException.ThrowIfNullOrWhiteSpace(StorageKey);
	}
}
