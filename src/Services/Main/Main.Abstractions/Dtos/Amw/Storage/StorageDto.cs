namespace Main.Abstractions.Dtos.Amw.Storage;

public class StorageDto
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string Type { get; set; } = null!;
}