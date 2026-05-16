using System.ComponentModel.DataAnnotations;

namespace Cache;

public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public required string Url { get; init; }

    [Required]
    public required string Password { get; init; }

    public string ConnectionString => $"{Url},password={Password}";
}