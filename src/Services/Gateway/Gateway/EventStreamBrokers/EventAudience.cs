namespace Gateway.EventStreamBrokers;

public readonly record struct EventAudience(Guid? UserId)
{
	public static readonly EventAudience Global = new(null);
	public bool IsGlobal => UserId is null;

	public static EventAudience ForUser(Guid userId)
	{
		ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
		return new(userId);
	}
}
