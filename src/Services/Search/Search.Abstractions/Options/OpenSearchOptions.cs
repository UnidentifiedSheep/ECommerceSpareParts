using System.ComponentModel.DataAnnotations;

namespace Search.Abstractions.Options;

public record OpenSearchOptions
{
    public const string SectionName = "OpenSearch";

    [Required]
    public required string Uri { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public bool AllowInvalidCertificate { get; init; }

    [Required]
    public required OpenSearchIndexOptions IndexOptions { get; init; }
}