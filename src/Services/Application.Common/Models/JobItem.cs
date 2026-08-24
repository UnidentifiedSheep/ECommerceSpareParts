namespace Application.Common.Models;

public interface IJobItem
{
    public string SystemName { get; }
    public string InputState { get; }
    public int MaxAttempts { get; }
    public string? NaturalKey { get; }
}

public sealed record JobItem(
    string SystemName,
    string InputState,
    int MaxAttempts,
    string? NaturalKey = null) : IJobItem;

public sealed record UniqJobItem(
    string SystemName,
    string InputState,
    int MaxAttempts,
    string NaturalKey) : IJobItem;
