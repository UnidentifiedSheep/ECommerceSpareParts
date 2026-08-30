namespace Application.Common.Models;

public interface IJobItem
{
	string SystemName { get; }

	string InputState { get; }

	int MaxAttempts { get; }

	string? NaturalKey { get; }
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
