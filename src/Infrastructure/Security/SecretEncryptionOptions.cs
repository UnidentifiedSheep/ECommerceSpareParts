using System.ComponentModel.DataAnnotations;

namespace Security;

public record SecretEncryptionOptions
{
    public const string SectionName = "SecretEncryption";

    [Required]
    public string Secret { get; set; } = string.Empty;
}