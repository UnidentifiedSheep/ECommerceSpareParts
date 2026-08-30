using Application.Common.Models;

namespace Application.Common.Interfaces.Events;

public interface IIntegrationEventScope
{
	void Add<T>(T @event, string? routingKey = null);

	void AddRange<T>(IEnumerable<T> events, string? routingKey = null);

	IReadOnlyCollection<IntegrationEventEnvelope> Flush();
}
