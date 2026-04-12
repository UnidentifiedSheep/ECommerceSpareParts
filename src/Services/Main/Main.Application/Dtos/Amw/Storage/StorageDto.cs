using Main.Enums;

namespace Main.Abstractions.Dtos.Amw.Storage;

public class StorageDto
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public StorageType Type { get; set; }
}