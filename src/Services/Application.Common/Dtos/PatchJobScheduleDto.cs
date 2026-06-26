using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Application.Common.Dtos;

public record PatchJobScheduleDto
{
    [JsonPropertyName("name")]
    public PatchField<string> Name { get; init; } = PatchField<string>.NotSet();

    [JsonPropertyName("description")]
    public PatchField<string?> Description { get; init; } = PatchField<string?>.NotSet();

    [JsonPropertyName("inputState")]
    public PatchField<string> InputState { get; init; } = PatchField<string>.NotSet();

    [JsonPropertyName("maxAttempts")]
    public PatchField<int> MaxAttempts { get; init; } = PatchField<int>.NotSet();

    [JsonPropertyName("cron")]
    public PatchField<string> Cron { get; init; } = PatchField<string>.NotSet();

    [JsonPropertyName("enabled")]
    public PatchField<bool> Enabled { get; init; } = PatchField<bool>.NotSet();
}
