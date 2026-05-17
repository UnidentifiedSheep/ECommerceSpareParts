using System.ComponentModel.DataAnnotations;

namespace Cache;

public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public required string Url { get; init; }

    public string? Password { get; init; }

    public string ConnectionString => string.IsNullOrWhiteSpace(Password)
        ? Url
        : $"{Url},password={Password}";
}