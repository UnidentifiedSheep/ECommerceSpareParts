using BulkValidation.Core.Attributes;
using Main.Enums;

namespace Main.Entities.Storage;

public class Storage
{
    [Validate]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public StorageType Type { get; set; }
}