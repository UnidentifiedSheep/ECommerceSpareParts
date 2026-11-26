using System.Text.Json.Serialization;
using Integrations.Armtek.Models;

namespace Integrations.Armtek.Responses;

public abstract record ArmtekResponse
{
	public const int SuccessStatus = 200;

	[JsonPropertyName("STATUS")]
	public int Status { get; init; }

	[JsonPropertyName("MESSAGES")]
	public IReadOnlyList<ArmtekMessage>? Messages { get; init; }

	[JsonIgnore]
	public IReadOnlyList<ArmtekMessage> Errors => Messages?.Where(x => x.IsError).ToList() ?? [];

	[JsonIgnore]
	public bool IsSuccess => Status == SuccessStatus && Errors.Count == 0;
}
