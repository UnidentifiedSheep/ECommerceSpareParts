using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;

namespace Main.Entities.Producer;

public class Producer : AuditableEntity<Producer, int>
{
    private Producer()
    {
    }

    private Producer(string name, string? description = null, string? imagePath = null)
    {
        SetName(name);
        SetImagePath(imagePath);
        SetDescription(description);
    }

    [Validate]
    public int Id { get; private set; }

    [Validate]
    public string Name { get; private set; } = null!;

    public string? ImagePath { get; private set; }

    public string? Description { get; private set; }

    public static Producer Create(string name, string? description = null, string? imagePath = null)
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

    public void SetName(string name)
    {
        var value = name.Trim();

        value.AgainstNullOrWhiteSpace("producer.name.not.empty")
            .AgainstTooShort(2, "producer.name.min.length")
            .AgainstTooLong(64, "producer.name.max.length");

        Name = ToNormalizedName(value);
    }

    public static string ToNormalizedName(string value)
    {
        value = value.Trim();
        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    public override int GetId()
    {
        return Id;
    }
}