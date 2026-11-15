using System.Threading.Channels;

namespace Api.Common.HostedServices;

public interface IJobSignal
{
	void Notify();

	Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
}

public sealed class JobSignal : IJobSignal
{
	public const string Channel = "job_ready";

	public static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

	private readonly Channel<bool> _channel = System.Threading.Channels.Channel.CreateBounded<bool>(
		new BoundedChannelOptions(1)
		{
			FullMode = BoundedChannelFullMode.DropWrite,
			SingleReader = false,
			SingleWriter = false
		});

	public void Notify() => _channel.Writer.TryWrite(true);

	public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
	{
		using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutSource.CancelAfter(timeout);
		try
		{
			await _channel.Reader.ReadAsync(timeoutSource.Token);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { }
	}
}
