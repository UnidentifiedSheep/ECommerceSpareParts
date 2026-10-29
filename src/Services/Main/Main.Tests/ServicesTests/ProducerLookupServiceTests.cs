using FluentAssertions;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Producer;
using Main.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.ServicesTests;

public sealed class ProducerLookupServiceTests
{
	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	[Fact]
	public async Task ConcurrentRequestsAcrossScopes_ShareOneLoadAndSnapshot()
	{
		var completion = Completion();
		var calls = 0;
		await using var root = CreateProvider(_ =>
		{
			Interlocked.Increment(ref calls);
			return completion.Task;
		});
		await using var firstScope = root.CreateAsyncScope();
		await using var secondScope = root.CreateAsyncScope();
		var first = firstScope.ServiceProvider.GetRequiredService<IProducerLookupService>().Load(CancellationToken);
		var second = secondScope.ServiceProvider.GetRequiredService<IProducerLookupService>().Load(CancellationToken);

		calls.Should().Be(1);
		completion.SetResult(Snapshot(1));
		(await first).Should().BeSameAs(await second);
	}

	[Fact]
	public async Task AfterInvalidation_NewRequestsWaitAndOldSnapshotRemainsUsable()
	{
		var completion = Completion();
		var calls = 0;
		await using var root = CreateProvider(_ => ++calls == 1 ? Task.FromResult(Snapshot(1)) : completion.Task);
		var service = root.GetRequiredService<IProducerLookupService>();
		var old = await service.Load(CancellationToken);

		service.Invalidate();
		var refreshing = service.Load(CancellationToken);
		refreshing.IsCompleted.Should().BeFalse();
		old.ResolveId("producer").Should().Be(1);
		completion.SetResult(Snapshot(2));

		var current = await refreshing;
		current.ResolveId("producer").Should().Be(2);
		(await service.Load(CancellationToken)).Should().BeSameAs(current);
		old.ResolveId("producer").Should().Be(1);
		calls.Should().Be(2);
	}

	[Fact]
	public async Task InvalidationDuringLoad_DiscardsObsoleteResultAndLoadsAgain()
	{
		var firstCompletion = Completion();
		var secondCompletion = Completion();
		var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var calls = 0;
		await using var root = CreateProvider(_ =>
		{
			if (++calls == 1)
				return firstCompletion.Task;
			secondStarted.SetResult();
			return secondCompletion.Task;
		});
		var service = root.GetRequiredService<IProducerLookupService>();
		var first = service.Load(CancellationToken);
		service.Invalidate();
		var second = service.Load(CancellationToken);
		firstCompletion.SetResult(Snapshot(1));
		await secondStarted.Task.WaitAsync(CancellationToken);
		first.IsCompleted.Should().BeFalse();
		second.IsCompleted.Should().BeFalse();
		secondCompletion.SetResult(Snapshot(2));

		(await first).ResolveId("producer").Should().Be(2);
		(await second).Should().BeSameAs(await first);
		calls.Should().Be(2);
	}

	[Fact]
	public async Task CancelledWaiter_DoesNotCancelSharedLoadForOtherRequests()
	{
		var completion = Completion();
		CancellationToken loadingToken = default;
		await using var root = CreateProvider(token =>
		{
			loadingToken = token;
			return completion.Task;
		});
		var service = root.GetRequiredService<IProducerLookupService>();
		using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
		var first = service.Load(cancelled.Token);
		var second = service.Load(CancellationToken);
		await cancelled.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
		loadingToken.IsCancellationRequested.Should().BeFalse();
		completion.SetResult(Snapshot(1));
		(await second).ResolveId("producer").Should().Be(1);
	}

	[Fact]
	public async Task FailedLoad_IsRetriedByNextRequest()
	{
		var calls = 0;
		await using var root = CreateProvider(_ => ++calls == 1
			? Task.FromException<IProducerLookup>(new InvalidOperationException("Database unavailable"))
			: Task.FromResult(Snapshot(1)));
		var service = root.GetRequiredService<IProducerLookupService>();

		await Assert.ThrowsAsync<InvalidOperationException>(() => service.Load(CancellationToken));
		(await service.Load(CancellationToken)).ResolveId("producer").Should().Be(1);
		calls.Should().Be(2);
	}

	[Fact]
	public void Snapshot_DoesNotRetainMutableInputDictionaries()
	{
		var names = new Dictionary<string, int> { ["PRODUCER"] = 1 };
		var aliases = new Dictionary<string, int> { ["ALIAS"] = 1 };
		var mappings = new Dictionary<ProducerSupplierLookupKey, int>
		{
			[new ProducerSupplierLookupKey(Enums.Supplier.Armtek, "SUPPLIER NAME")] = 1
		};
		var snapshot = new SupplierProducerLookup(new ProducerLookup(names, aliases), mappings);

		names.Clear();
		aliases.Clear();
		mappings.Clear();

		snapshot.ResolveId("producer").Should().Be(1);
		snapshot.ResolveId("alias").Should().Be(1);
		snapshot.ResolveId("supplier name", Enums.Supplier.Armtek).Should().Be(1);
	}

	private static TaskCompletionSource<IProducerLookup> Completion() => new(TaskCreationOptions.RunContinuationsAsynchronously);

	private static IProducerLookup Snapshot(int id) => new ProducerLookup(
		new Dictionary<string, int> { ["PRODUCER"] = id }, new Dictionary<string, int>());

	private static ServiceProvider CreateProvider(Func<CancellationToken, Task<IProducerLookup>> load) => new ServiceCollection()
		.AddSingleton<IProducerLookupService, ProducerLookupService>()
		.AddScoped<IProducerLookupLoader>(_ => new DelegateLoader(load))
		.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

	private sealed class DelegateLoader(Func<CancellationToken, Task<IProducerLookup>> load) : IProducerLookupLoader
	{
		public Task<IProducerLookup> Load(CancellationToken cancellationToken = default) => load(cancellationToken);
	}
}
