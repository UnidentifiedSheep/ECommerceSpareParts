namespace Domain.CommonEntities.History;

public sealed record EntityHistoryReason(string Code, string? ReferenceType = null, string? ReferenceKey = null);
