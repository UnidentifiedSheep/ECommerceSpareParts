using System.ComponentModel.DataAnnotations;

namespace Search.Abstractions.Options;

public record OpenSearchIndexOptions
{
    [Required]
    public required string Products { get; init; }
}