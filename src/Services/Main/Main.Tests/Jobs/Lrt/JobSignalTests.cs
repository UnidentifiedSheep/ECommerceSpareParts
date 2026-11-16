using Api.Common.HostedServices;
using FluentAssertions;
using Main.Modules.Jobs.Application.HostedServices;

namespace Tests.Jobs.Lrt;

public class JobSignalTests
{
	[Fact]
	public async Task WaitAsync_ReturnsImmediatelyAfterNotify()
	{
		var signal = new JobSignal();
		signal.Notify();

		var started = DateTime.UtcNow;
		await signal.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

		(DateTime.UtcNow - started).Should().BeLessThan(TimeSpan.FromSeconds(5));
	}

	[Fact]
	public async Task WaitAsync_ReturnsAfterTimeoutWithoutNotify()
	{
		var signal = new JobSignal();

		var started = DateTime.UtcNow;
		await signal.WaitAsync(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);

		(DateTime.UtcNow - started).Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150));
	}

	[Fact]
	public async Task Notify_CoalescesBurstIntoSingleWakeUp()
	{
		var signal = new JobSignal();
		signal.Notify();
		signal.Notify();
		signal.Notify();

		await signal.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

		var started = DateTime.UtcNow;
		await signal.WaitAsync(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
		(DateTime.UtcNow - started).Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150));
	}

	[Fact]
	public async Task WaitAsync_PropagatesCallerCancellation()
	{
		var signal = new JobSignal();
		using var source = new CancellationTokenSource();
		await source.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => signal.WaitAsync(TimeSpan.FromSeconds(30), source.Token));
	}
}
