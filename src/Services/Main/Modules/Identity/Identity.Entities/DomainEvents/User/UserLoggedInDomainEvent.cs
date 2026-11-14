using System.Net;
using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.User;

public record UserLoggedInDomainEvent : IKeyedDomainEvent, IBatchableDomainEvent
{
	public UserLoggedInDomainEvent(
		Guid userId,
		DateTime occurredAtUtc,
		IPAddress? ipAddress,
		string? userAgent)
	{
		UserId = userId;
		OccurredAtUtc = occurredAtUtc;
		IpAddress = ipAddress;
		UserAgent = userAgent;
	}

	public Guid UserId { get; }

	public DateTime OccurredAtUtc { get; }

	public IPAddress? IpAddress { get; }

	public string? UserAgent { get; }

	public string GetKey() => $"user:{UserId}:logged:in";
}
