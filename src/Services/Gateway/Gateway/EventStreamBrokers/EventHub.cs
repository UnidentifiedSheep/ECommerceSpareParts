using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using HotChocolate.Fusion.Subscriptions;

namespace Gateway.EventStreamBrokers;

public interface IEventHub : IDisposable
{
	IAsyncEnumerable<EventMessage> SubscribeAsync(
		string[] topics,
		EventAudience audience,
		CancellationToken cancellationToken);


	void Publish<TEvent>(TEvent @event, EventAudience audience);
}

public sealed class EventHub(IEventRegistry registry) : IEventHub
{
	private readonly Lock _disposeGate = new();
	private readonly ConcurrentDictionary<Session, byte> _sessions = new();
	private volatile bool _disposed;

	public IAsyncEnumerable<EventMessage> SubscribeAsync(
		string[] topics,
		EventAudience audience,
		CancellationToken cancellationToken)
		=> ReadAsync(new Session(topics, audience), cancellationToken);

	public void Publish<TEvent>(TEvent @event, EventAudience audience)
	{
		ArgumentNullException.ThrowIfNull(@event);
		ObjectDisposedException.ThrowIf(_disposed, this);

		var descriptor = registry.Get<TEvent>();
		var payload = JsonSerializer.SerializeToUtf8Bytes(descriptor.Map(@event));

		foreach (var session in _sessions.Keys)
		{
			if (!session.Topics.Contains(descriptor.Topic))
				continue;

			if (!audience.IsGlobal && session.Audience != audience)
				continue;

			session.TryPublish(payload);
		}
	}

	private async IAsyncEnumerable<EventMessage> ReadAsync(
		Session session,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		AddSession(session);

		try
		{
			await foreach (var message in ReadMessagesAsync(session.Channel.Reader, cancellationToken))
				yield return message;
		}
		finally
		{
			_sessions.TryRemove(session, out _);
			session.Dispose();
		}
	}

	private void AddSession(Session session)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (!_sessions.TryAdd(session, 0))
			throw new InvalidOperationException("The event stream session is already registered.");

		if (!_disposed) return;

		_sessions.TryRemove(session, out _);
		session.Dispose();
		throw new ObjectDisposedException(nameof(EventHub));
	}

	private static async IAsyncEnumerable<EventMessage> ReadMessagesAsync(
		ChannelReader<EventMessage> reader,
		[EnumeratorCancellation]
		CancellationToken cancellationToken)
	{
		while (true)
		{
			bool ready;
			try
			{
				ready = await reader.WaitToReadAsync(cancellationToken);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			if (!ready) break;

			while (reader.TryRead(out var message))
				yield return message;
		}
	}

	public void Dispose()
	{
		lock (_disposeGate)
		{
			if (_disposed)
				return;

			_disposed = true;
		}

		foreach (var (session, _) in _sessions)
			if (_sessions.TryRemove(session, out _))
				session.Close();
	}

	private sealed class Session(string[] topics, EventAudience audience) : IDisposable
	{
		private volatile bool _closed;

		public EventAudience Audience => audience;
		public HashSet<string> Topics { get; } = topics.ToHashSet(StringComparer.Ordinal);
		public Channel<EventMessage> Channel { get; } = System.Threading.Channels.Channel
			.CreateBounded<EventMessage>(
				new BoundedChannelOptions(256) //TODO: maybe we should take it from options?
				{
					SingleReader = true,
					SingleWriter = false,
					FullMode = BoundedChannelFullMode.Wait
				});

		public void TryPublish(ReadOnlySpan<byte> payload)
		{
			if (_closed) return;

			var owner = MemoryPool<byte>.Shared.Rent(payload.Length);
			payload.CopyTo(owner.Memory.Span);
			var message = new EventMessage(owner, ..payload.Length, ..0);

			if (Channel.Writer.TryWrite(message)) return;

			message.Dispose();
			Close(new InvalidOperationException(
				"The event stream subscriber cannot keep up with incoming messages."));
		}

		public void Close(Exception? error = null)
		{
			_closed = true;
			Channel.Writer.TryComplete(error);
		}

		public void Dispose()
		{
			Close();
			while (Channel.Reader.TryRead(out var message))
				message.Dispose();
		}
	}
}
