namespace Application.Common.Dtos;

public record NamedObjectDto
{
    public required string SystemName { get; init; }
    public required string? Name { get; init; }
    public required string? Description { get; init; }
}