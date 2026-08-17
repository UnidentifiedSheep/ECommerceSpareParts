using System.Text.Json.Serialization;
using Abstractions.Models;

namespace Main.Application.Dtos.Organizations;

public record PatchOrganizationDto
{
    [JsonPropertyName("name")]
    public PatchField<string> Name { get; init; } = PatchField<string>.NotSet();
    
    [JsonPropertyName("isHidden")]
    public PatchField<bool> IsHidden { get; init; } = PatchField<bool>.NotSet();
}