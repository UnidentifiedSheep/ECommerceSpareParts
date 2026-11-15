namespace Persistence.Listening;

public sealed record ChannelNotification(string? Payload, bool IsResync);
