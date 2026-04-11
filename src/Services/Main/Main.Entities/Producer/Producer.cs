using BulkValidation.Core.Attributes;

namespace Main.Entities;

public class Producer
{
    [Validate]
    public int Id { get; set; }

    [Validate]
    public string Name { get; set; } = null!;

    public bool IsOe { get; set; }

    public string? ImagePath { get; set; }

    public string? Description { get; set; }
}