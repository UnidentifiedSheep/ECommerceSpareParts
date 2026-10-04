using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Common.LRT;

public static class LrtStateSerializer
{
	private const int CurrentVersion = 1;
	private const string VersionProperty = "$lrtVersion";

	public static (TInputState Input, TState? State) Deserialize<TInputState, TState>(string json)
		where TInputState : class
		where TState : class
	{
		using var document = JsonDocument.Parse(json);
		if (IsEnvelope(document.RootElement))
		{
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

	public static TState? DeserializeState<TState>(string json) where TState : class
	{
		using var document = JsonDocument.Parse(json);
		var root = document.RootElement;
		var state = IsEnvelope(root) ? root.GetProperty("state") : root;
		return state.Deserialize<TState>();
	}

	public static string Serialize<TInputState, TState>(TInputState input, TState? state)
		where TInputState : class
		where TState : class
		=> JsonSerializer.Serialize(new Envelope<TInputState, TState>(CurrentVersion, input, state));

	private static bool IsEnvelope(JsonElement root)
	{
		if (root.ValueKind != JsonValueKind.Object ||
		    !root.TryGetProperty(VersionProperty, out var version))
			return false;

		if (version.ValueKind != JsonValueKind.Number || version.GetInt32() != CurrentVersion)
			throw new JsonException("Unsupported LRT state version.");

		return true;
	}

	private sealed record Envelope<TInputState, TState>(
		[property: JsonPropertyName(VersionProperty)] int Version,
		[property: JsonPropertyName("input")] TInputState Input,
		[property: JsonPropertyName("state")] TState? State);
}
