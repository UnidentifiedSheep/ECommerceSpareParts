using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;

namespace Main.Application.Lrts.RunServiceTask;

public sealed record RunServiceTaskInputState : IInputState
{
	[JsonPropertyName("serviceName")]
	public required string ServiceName { get; init; }

	[JsonPropertyName("taskSystemName")]
	public required string TaskSystemName { get; init; }

	public void ValidateState()
	{
		if (string.IsNullOrWhiteSpace(ServiceName))
			throw new InvalidOperationException("Service name is required.");
		if (string.IsNullOrWhiteSpace(TaskSystemName))
			throw new InvalidOperationException("Task system name is required.");
	}
}
