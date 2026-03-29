namespace Main.Abstractions.Dtos.RepositoryOptionsData;

public record GetUsersBySimilarityOptionsData
{
    public required double SimilarityLevel { get; init; }
    public string? Name {get; init; }
    public string? Surname { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? UserName { get; init; }
    public Guid? Id { get; init; }
    public string? Description { get; init; }
    public bool? IsSupplier { get; init; }
}