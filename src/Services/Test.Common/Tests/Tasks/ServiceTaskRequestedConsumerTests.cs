using Application.Common.Consumer;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Contracts.Tasks;
using FluentAssertions;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NamedObject;
using NamedObject.Core.Interfaces;

namespace Tests.Tests.Tasks;

public sealed class ServiceTaskRequestedConsumerTests
{
	[Fact]
	public async Task Consumer_ResolvesTaskByNameWithoutCaseSensitivityAndPassesCancellationToken()
	{
		await using var provider = CreateProvider();
		await using var scope = provider.CreateAsyncScope();
		var task = GetTask(scope.ServiceProvider);
		using var cancellation = new CancellationTokenSource();
		CancellationToken? receivedToken = null;
		task.Work = token =>
		{
			receivedToken = token;
			return Task.CompletedTask;
		};

		await scope.ServiceProvider.GetRequiredService<ServiceTaskRequestedConsumer>()
			.Consume(Context(task.SystemName.ToUpperInvariant(), cancellation.Token));

		task.Executions.Should().Be(1);
		task.ReceivedJobId.Should().Be(Guid.Empty);
		task.ReceivedLeaseHolderId.Should().Be(Guid.Empty);
		receivedToken.Should().Be(cancellation.Token);
	}

	[Fact]
	public async Task Consumer_WaitsUntilTheLocalTaskCompletes()
	{
		await using var provider = CreateProvider();
		await using var scope = provider.CreateAsyncScope();
		var task = GetTask(scope.ServiceProvider);
		var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		task.Work = _ => completion.Task;

		var consumption = scope.ServiceProvider.GetRequiredService<ServiceTaskRequestedConsumer>()
			.Consume(Context(task.SystemName, TestContext.Current.CancellationToken));

		consumption.IsCompleted.Should().BeFalse();
		completion.SetResult();
		await consumption;
		task.Executions.Should().Be(1);
	}

	[Fact]
	public async Task Consumer_PropagatesTaskFailureToTheBroker()
	{
		await using var provider = CreateProvider();
		await using var scope = provider.CreateAsyncScope();
		var task = GetTask(scope.ServiceProvider);
		var failure = new InvalidOperationException("Task failed.");
		task.Work = _ => Task.FromException(failure);

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
			scope.ServiceProvider.GetRequiredService<ServiceTaskRequestedConsumer>().Consume(Context(task.SystemName, TestContext.Current.CancellationToken)));

		exception.Should().BeSameAs(failure);
	}

	[Fact]
	public async Task Consumer_RejectsAnUnknownTaskWithoutExecutingOtherTasks()
	{
		await using var provider = CreateProvider();
		await using var scope = provider.CreateAsyncScope();

		await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
			await scope.ServiceProvider.GetRequiredService<ServiceTaskRequestedConsumer>().Consume(Context("unknown-task", TestContext.Current.CancellationToken)));

		GetTask(scope.ServiceProvider).Executions.Should().Be(0);
	}

	[Fact]
	public async Task Consumer_DoesNotStartWorkWhenCancellationIsAlreadyRequested()
	{
		await using var provider = CreateProvider();
		await using var scope = provider.CreateAsyncScope();
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
			await scope.ServiceProvider.GetRequiredService<ServiceTaskRequestedConsumer>()
				.Consume(Context(nameof(RecordingServiceTask), cancellation.Token)));

		GetTask(scope.ServiceProvider).Executions.Should().Be(0);
	}

	[Theory]
	[InlineData("")]
	[InlineData("  ")]
	public async Task Consumer_RejectsAnEmptyName(string name)
	{
		await using var provider = CreateProvider();
		await using var scope = provider.CreateAsyncScope();

		await Assert.ThrowsAsync<ArgumentException>(async () =>
			await scope.ServiceProvider.GetRequiredService<ServiceTaskRequestedConsumer>()
				.Consume(Context(name, TestContext.Current.CancellationToken)));
	}

	[Fact]
	public async Task Registration_CreatesSeparateTaskInstancesForEachScope()
	{
		await using var provider = CreateProvider();
		await using var first = provider.CreateAsyncScope();
		await using var second = provider.CreateAsyncScope();

		GetTask(first.ServiceProvider).Should().BeSameAs(GetTask(first.ServiceProvider));
		GetTask(first.ServiceProvider).Should().NotBeSameAs(GetTask(second.ServiceProvider));
	}

	private static ServiceProvider CreateProvider()
	{
		var services = new ServiceCollection();
		services.AddScoped<ILrtNamedObject, RecordingServiceTask>();
		services.AddNamedObjectRegistry();
		services.AddTransient<ServiceTaskRequestedConsumer>();
		return services.BuildServiceProvider(validateScopes: true);
	}

	private static RecordingServiceTask GetTask(IServiceProvider provider) =>
		(RecordingServiceTask)provider.GetRequiredService<INamedObjectRegistry<ILrtNamedObject>>()
			.GetBySystemName(nameof(RecordingServiceTask));

	private static ConsumeContext<ServiceTaskRequestedEvent> Context(string name, CancellationToken token = default)
	{
		var context = new Mock<ConsumeContext<ServiceTaskRequestedEvent>>();
		context.SetupGet(value => value.Message).Returns(new ServiceTaskRequestedEvent { TaskSystemName = name });
		context.SetupGet(value => value.CancellationToken).Returns(token);
		return context.Object;
	}

	public sealed class RecordingServiceTask : ILrtNamedObject
	{
		public string SystemName => nameof(RecordingServiceTask);
		public ILocalizableMessage NameLocalizationMessage => new LocalizableMessage("test-task-name");
		public ILocalizableMessage DescriptionLocalizationMessage => new LocalizableMessage("test-task-description");
		public Type InputType => typeof(NoneInputState);
		public Type StateType => typeof(NoneInputState);
		public int Executions { get; private set; }
		public Guid? ReceivedJobId { get; private set; }
		public Guid? ReceivedLeaseHolderId { get; private set; }
		public Func<CancellationToken, Task> Work { get; set; } = _ => Task.CompletedTask;

		public Task ExecuteAsync(
			Guid jobId,
			Guid leaseHolderId,
			CancellationToken cancellationToken = default)
		{
			Executions++;
			ReceivedJobId = jobId;
			ReceivedLeaseHolderId = leaseHolderId;
			return Work(cancellationToken);
		}
	}
}
