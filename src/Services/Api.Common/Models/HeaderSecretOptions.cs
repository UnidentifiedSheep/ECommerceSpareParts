using System.ComponentModel.DataAnnotations;

namespace Api.Common.Models;

public class HeaderSecretOptions
{
    public const string SectionName = "HeaderSecret";
    [Required]
    public required string Key { get; init; }
}