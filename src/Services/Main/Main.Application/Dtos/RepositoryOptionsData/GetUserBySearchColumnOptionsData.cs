namespace Main.Application.Dtos.RepositoryOptionsData;

public record GetUserBySearchColumnOptionsData
{
    public string? SearchTerm { get; init; }

    public bool? IsSupplier { get; init; }
}