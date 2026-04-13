using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Main.Entities.Producer.ValueObjects;

namespace Main.Entities.Producer;

public class Producer : AuditableEntity<Producer, int>
{
    [Validate]
    public int Id { get; private set; }

    [Validate]
    public Name Name { get; private set; } = null!;

    public string? ImagePath { get; private set; }

    public string? Description { get; private set; }
    
    private Producer() { }

    private Producer(Name name, string? description = null, string? imagePath = null)
    {
        Name = name;
        SetImagePath(imagePath);
        SetDescription(description);
    }

    public static Producer Create(Name name, string? description = null, string? imagePath = null)
    {
        return new Producer(name, description, imagePath);
    }

    public void SetImagePath(string? imagePath)
    {
        imagePath = imagePath?.Trim()
            .AgainstTooLong(255, "producer.image.too.long");
        
        ImagePath = string.IsNullOrEmpty(imagePath) ? null : imagePath;
    }

    public void SetDescription(string? description)
    {
        description = description?.Trim()
            .AgainstTooLong(500, "producer.description.max.length");

        Description = string.IsNullOrEmpty(description) ? null : description;
    }

    public void SetName(Name name)
    {
        Name = name;
    }
    
    public override int GetId() => Id;
}