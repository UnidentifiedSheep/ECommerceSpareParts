using System.Runtime.CompilerServices;
using HotChocolate.Fusion.Subscriptions;
using Security.Extensions;

namespace Gateway.EventStreamBrokers;

public sealed class RabbitmqEventStreamBroker(
	IEventHub hub,
	IEventRegistry registry,
	IHttpContextAccessor httpContextAccessor) : IEventStreamBroker
{
	private readonly CancellationTokenSource _lifetime = new();
	private bool _disposed;

	public IAsyncEnumerable<EventMessage> SubscribeAsync(
		ISubscriptionFieldContext context,
		string[] topics,
		string? cursor,
		CancellationToken cancellationToken)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(topics);
		ArgumentOutOfRangeException.ThrowIfZero(topics.Length);

		if (!string.IsNullOrEmpty(cursor))
			throw new InvalidEventMessageCursorException();

		foreach (var topic in topics)
		{
			ArgumentException.ThrowIfNullOrEmpty(topic);
			registry.Get(topic);
		}

		var principal = httpContextAccessor.GetPrincipal();

		var audience = principal?.Identity?.IsAuthenticated == true
			? EventAudience.ForUser(principal.GetUserId() ?? throw new InvalidOperationException(
				"Authenticated principal does not contain a user Id."))
			: EventAudience.Global;

		return ReadAsync(topics, audience, _lifetime.Token, cancellationToken);
	}

	private async IAsyncEnumerable<EventMessage> ReadAsync(
		string[] topics,
		EventAudience audience,
		CancellationToken lifetimeToken,
		[EnumeratorCancellation]
		CancellationToken cancellationToken)
	{
		using var linked = CancellationTokenSource.CreateLinkedTokenSource(
			lifetimeToken,
			cancellationToken);

		await foreach (var message in hub.SubscribeAsync(topics, audience, linked.Token))
			yield return message;
	}

	public ValueTask DisposeAsync()
	{
		if (_disposed)
			return ValueTask.CompletedTask;

		_disposed = true;
		_lifetime.Cancel();
		_lifetime.Dispose();

		return ValueTask.CompletedTask;
	}
}

public sealed class RabbitmqEventStreamBrokerFactory(
	IEventHub hub,
	IEventRegistry registry,
	IHttpContextAccessor httpContextAccessor) : IEventStreamBrokerFactory
{
	public IEventStreamBroker Create(string? broker)
		=> new RabbitmqEventStreamBroker(hub, registry, httpContextAccessor);
}
