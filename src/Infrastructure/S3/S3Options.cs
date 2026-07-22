using System.ComponentModel.DataAnnotations;

namespace S3;

public class S3Options
{
    public const string SectionName = "S3";

    [Required]
    public required string InternalUrl { get; init; }

    [Required]
    public required string ExternalUrl { get; init; }

    public string Region { get; init; } = "";

    [Required]
    public required string Login { get; init; }

    [Required]
    public required string Password { get; init; }

    [Required]
    public required bool ForcePathStyle { get; init; }
}