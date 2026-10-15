namespace Contracts.Tasks;

public sealed record ServiceTaskRequestedEvent
{
	public required string TaskSystemName { get; init; }
}
