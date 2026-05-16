using System.ComponentModel.DataAnnotations;

namespace Internal.Integration.Core;

public record InternalServiceCredentials
{
    public const string SectionName = "InternalServiceCredentials";

    [Required]
    public required string Service { get; init; }

    [Required]
    public required string Secret { get; init; }
}