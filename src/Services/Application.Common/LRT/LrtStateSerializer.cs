using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Common.LRT;

internal static class LrtStateSerializer
{
	private const int CurrentVersion = 1;
	private const string VersionProperty = "$lrtVersion";

	public static (TInputState Input, TState? State) Deserialize<TInputState, TState>(string json)
		where TInputState : class
		where TState : class
	{
		using var document = JsonDocument.Parse(json);
		if (document.RootElement.ValueKind == JsonValueKind.Object &&
			document.RootElement.TryGetProperty(VersionProperty, out var version))
		{
			if (version.ValueKind != JsonValueKind.Number || version.GetInt32() != CurrentVersion)
				throw new JsonException("Unsupported LRT state version.");

			var envelope = document.RootElement.Deserialize<Envelope<TInputState, TState>>() ??
				throw new JsonException("LRT state envelope is empty.");
			return (envelope.Input ?? throw new JsonException("LRT input state is missing."),
				envelope.State);
		}

		var input = document.RootElement.Deserialize<TInputState>() ??
			throw new JsonException("LRT input state is empty.");
		var state = document.RootElement.Deserialize<TState>();
		return (input, state);
	}

	public static string Serialize<TInputState, TState>(TInputState input, TState? state)
		where TInputState : class
		where TState : class
		=> JsonSerializer.Serialize(new Envelope<TInputState, TState>(CurrentVersion, input, state));

	private sealed record Envelope<TInputState, TState>(
		[property: JsonPropertyName(VersionProperty)] int Version,
		[property: JsonPropertyName("input")] TInputState Input,
		[property: JsonPropertyName("state")] TState? State);
}
