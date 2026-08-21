namespace Application.Common.Models.Jobs;

public sealed record JobItem(
    string SystemName,
    string InputState,
    int MaxAttempts);
