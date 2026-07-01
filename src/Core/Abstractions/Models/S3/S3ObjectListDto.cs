namespace Abstractions.Models.S3;

public record S3ObjectListDto
{
    public required IReadOnlyList<S3ObjectDto> Files { get; init; }

    public string? NextContinuationToken { get; init; }

    public required bool HasMore { get; init; }
}