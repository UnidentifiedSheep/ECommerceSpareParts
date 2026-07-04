using System.ComponentModel.DataAnnotations;

namespace Main.Migrator;

public record ServiceSecrets
{
    public const string SectionName = "ServiceSecrets";

    [Required]
    public required string MainApp { get; init; }

    [Required]
    public required string Analytics { get; init; }

    [Required]
    public required string Pricing { get; init; }

    [Required]
    public required string Search { get; init; }

    [Required]
    public required string Gateway { get; init; }
}