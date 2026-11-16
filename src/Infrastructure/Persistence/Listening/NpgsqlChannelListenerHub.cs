using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Persistence.Listening;

public sealed class NpgsqlChannelListenerHub(IOptions<DatabaseOptions> options,
	ILogger<NpgsqlChannelListenerHub> logger)
	: IChannelListenerFactory, IAsyncDisposable
{
	public const string ApplicationName = "channel-listener-hub";

	private const int SubscriptionCapacity = 1024;
	private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

	private readonly Dictionary<string, List<Subscription>> _channels = new(StringComparer.Ordinal);
	private readonly Lock _gate = new();
	private readonly CancellationTokenSource _stop = new();
	private bool _changed;
	private bool _disposed;
	private Task? _loop;
	private CancellationTokenSource? _waitSource;

	public async ValueTask DisposeAsync()
	{
		Task? loop;
		List<Subscription> subscriptions;
		lock (_gate)
		{
			if (_disposed) return;
			_disposed = true;
			loop = _loop;
			subscriptions = _channels.Values.SelectMany(x => x).ToList();
		}

		await _stop.CancelAsync();
		if (loop is not null)
			await Task.WhenAny(loop, Task.Delay(TimeSpan.FromSeconds(5)));

		foreach (var subscription in subscriptions)
			subscription.Complete();
		_stop.Dispose();
	}

	public async Task<IChannelListener> ListenAsync(
		string channel,
		CancellationToken cancellationToken = default)
	{
		ChannelNames.Validate(channel);
		var subscription = new Subscription(this, channel);

		lock (_gate)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			if (!_channels.TryGetValue(channel, out var list))
				_channels[channel] = list = [];
			list.Add(subscription);
#pragma warning disable CA2016
			_loop ??= Task.Run(RunAsync);
#pragma warning restore CA2016
		}

		Wake();

		try
		{
			await subscription.Ready.WaitAsync(cancellationToken);
		}
		catch
		{
			await subscription.DisposeAsync();
			throw;
		}

		return subscription;
	}

	private async Task RunAsync()
	{
		while (!_stop.IsCancellationRequested)
			try
			{
				await ListenLoopAsync(_stop.Token);
			}
			catch (OperationCanceledException) when (_stop.IsCancellationRequested)
			{
				return;
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Channel listener connection failed. Reconnecting.");
				try
				{
					await Task.Delay(ReconnectDelay, _stop.Token);
				}
				catch (OperationCanceledException)
				{
					return;
				}
			}
	}

	private async Task ListenLoopAsync(CancellationToken ct)
	{
		var builder = new NpgsqlConnectionStringBuilder(options.Value.ConnectionString)
		{
			Pooling = false,
			KeepAlive = 30,
			ApplicationName = ApplicationName
		};

		await using var connection = new NpgsqlConnection(builder.ConnectionString);
		connection.Notification += (_, args) => Dispatch(args.Channel, args.Payload);

		var listening = new HashSet<string>(StringComparer.Ordinal);
		var isOpened = false;
		var firstPass = true;

		while (!ct.IsCancellationRequested)
		{
			lock (_gate)
				_changed = false;

			string[] desired;
			lock (_gate)
				desired = _channels.Where(x => x.Value.Count > 0).Select(x => x.Key).ToArray();

			if (desired.Length == 0)
			{
				if (isOpened)
					return;

				await WaitForChangeAsync(null, ct);
				continue;
			}

			if (!isOpened)
			{
				await connection.OpenAsync(ct);
				isOpened = true;
			}

			foreach (var channel in listening.Where(x => !desired.Contains(x)).ToArray())
			{
				await ExecuteAsync(
					connection,
					$"UNLISTEN {ChannelNames.Quote(channel)}",
					ct);
				listening.Remove(channel);
			}

			foreach (var channel in desired.Where(x => !listening.Contains(x)).ToArray())
			{
				await ExecuteAsync(
					connection,
					$"LISTEN {ChannelNames.Quote(channel)}",
					ct);
				listening.Add(channel);
			}

			PublishReadiness(firstPass);
			firstPass = false;

			await WaitForChangeAsync(connection, ct);
		}
	}

	private async Task WaitForChangeAsync(NpgsqlConnection? connection, CancellationToken ct)
	{
		CancellationTokenSource source;
		lock (_gate)
		{
			if (_changed)
				return;
			source = _waitSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
		}

		try
		{
			if (connection is null)
				await Task.Delay(Timeout.Infinite, source.Token);
			else
				await connection.WaitAsync(source.Token);
		}
		catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
		finally
		{
			lock (_gate)
				_waitSource = null;
			source.Dispose();
		}
	}

	private void Wake()
	{
		lock (_gate)
		{
			_changed = true;
			_waitSource?.Cancel();
		}
	}

	private static async Task ExecuteAsync(
		NpgsqlConnection connection,
		string sql,
		CancellationToken ct)
	{
		await using var command = new NpgsqlCommand(sql, connection);
		await command.ExecuteNonQueryAsync(ct);
	}

	private void PublishReadiness(bool resyncAll)
	{
		Subscription[] subscriptions;
		lock (_gate)
			subscriptions = _channels.Values.SelectMany(x => x).ToArray();

		foreach (var subscription in subscriptions)
			subscription.MarkReady(resyncAll);
	}

	private void Dispatch(string channel, string payload)
	{
		Subscription[] subscriptions;
		lock (_gate)
			subscriptions = _channels.TryGetValue(channel, out var list) ? list.ToArray() : [];

		foreach (var subscription in subscriptions)
			subscription.Write(new ChannelNotification(payload, false));
	}

	private void Remove(Subscription subscription)
	{
		lock (_gate)
		{
			if (_channels.TryGetValue(subscription.Channel, out var list))
			{
				list.Remove(subscription);
				if (list.Count == 0)
					_channels.Remove(subscription.Channel);
			}
		}

		Wake();
	}

	private sealed class Subscription : IChannelListener
	{

		private readonly NpgsqlChannelListenerHub _hub;

		private readonly Channel<ChannelNotification> _queue =
			System.Threading.Channels.Channel.CreateBounded<ChannelNotification>(
				new BoundedChannelOptions(SubscriptionCapacity)
				{
					FullMode = BoundedChannelFullMode.DropOldest,
					SingleReader = true
				});

		private readonly TaskCompletionSource
			_ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

		private int _disposed;

		public Subscription(NpgsqlChannelListenerHub hub, string channel)
		{
			_hub = hub;
			Channel = channel;
		}

		public Task Ready => _ready.Task;

		public string Channel { get; }

		public IAsyncEnumerable<ChannelNotification> ReadAllAsync(
			CancellationToken cancellationToken = default)
			=> _queue.Reader.ReadAllAsync(cancellationToken);

		public ValueTask DisposeAsync()
		{
			if (Interlocked.Exchange(ref _disposed, 1) == 0)
			{
				_hub.Remove(this);
				Complete();
			}

			return ValueTask.CompletedTask;
		}

		public void MarkReady(bool resync)
		{
			var becameReady = _ready.TrySetResult();
			if (becameReady || resync)
				_queue.Writer.TryWrite(new ChannelNotification(null, true));
		}

		public void Write(ChannelNotification notification) => _queue.Writer.TryWrite(notification);

		public void Complete()
		{
			_queue.Writer.TryComplete();
			_ready.TrySetCanceled();
		}
	}
}
