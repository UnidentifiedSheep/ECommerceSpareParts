using System.ComponentModel.DataAnnotations;

namespace Localization.Domain;

public class LocalesOptions
{
    public const string SectionName = "Locales";
    
    [Required]
    public required string Default { get; init; }
    
    [Required]
    public required string[] Supported { get; init; }
}