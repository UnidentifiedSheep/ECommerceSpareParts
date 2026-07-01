using System.ComponentModel.DataAnnotations;

namespace Persistence;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public required string Host { get; init; }

    public int? Port { get; init; }

    [Required]
    public required string Database { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }

    public string ConnectionString =>
        $"Host={Host};{(Port is null ? "" : $"Port={Port};")}Database={Database};Username={Username};Password={Password}";
}