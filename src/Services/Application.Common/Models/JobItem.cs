namespace Application.Common.Models;

public interface IJobItem
{
    public string SystemName { get; }
    public string InputState { get; }
    public int MaxAttempts { get; }
}

public sealed record JobItem(
    string SystemName,
    string InputState,
    int MaxAttempts) : IJobItem;

public sealed record UniqJobItem(
    string SystemName,
    string InputState,
    int MaxAttempts,
    string NaturalKey) : IJobItem;
