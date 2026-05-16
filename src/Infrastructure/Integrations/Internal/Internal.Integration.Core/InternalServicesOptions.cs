using System.ComponentModel.DataAnnotations;

namespace Internal.Integration.Core;

public record InternalServicesOptions
{
    public const string SectionName = "InternalServices";

    [Required]
    public required ServiceOptions Main { get; init; }

    [Required]
    public required string InternalToken { get; init; }
}

public record ServiceOptions
{
    [Required]
    public required string Url { get; init; }
}