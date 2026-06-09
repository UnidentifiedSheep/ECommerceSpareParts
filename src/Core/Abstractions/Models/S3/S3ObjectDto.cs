namespace Abstractions.Models.S3;

public record S3ObjectDto
{
    public required long Size { get; init; }
    public required string Key { get; init; }
    public required DateTime? LastModified { get; init; }
}