using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Main.Entities.DomainEvents.Producer;

namespace Main.Entities.Producer;

public class Producer : AuditableEntity<Producer, int>, ILinqEntity<Producer, int>
{
    private readonly List<ProducerAlias> _aliases = [];

    private Producer() { }

    private Producer(
        string name,
        string? description = null,
        string? imagePath = null)
    {
        SetName(name, false);
        SetImagePath(imagePath, false);
        SetDescription(description, false);
        
        AddDomainEvent(new ProducerCreatedDomainEvent(this));
    }

    [Validate]
    public int Id { get; private set; }

    [Validate]
    public string Name { get; private set; } = null!;

    public string? ImagePath { get; private set; }

    public string? Description { get; private set; }
    public IReadOnlyCollection<ProducerAlias> Aliases => _aliases;

    public static Expression<Func<Producer, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<Producer, bool>> GetEqualityExpression(int key) { return x => x.Id == key; }

    public static Producer Create(
        string name,
        string? description = null,
        string? imagePath = null)
    {
        return new Producer(
            name,
            description,
            imagePath);
    }

    public void SetImagePath(string? imagePath) => SetImagePath(imagePath, true);
    private void SetImagePath(string? imagePath, bool addEvent)
    {
        imagePath = imagePath?.Trim()
            .AgainstTooLong(255, "producer.image.too.long");

        ImagePath = string.IsNullOrEmpty(imagePath) ? null : imagePath;
        if (addEvent) AddDomainEvent(new ProducerUpdatedDomainEvent(Id));
    }

    public void SetDescription(string? description) => SetDescription(description, true);
    private void SetDescription(string? description, bool addEvent)
    {
        description = description?.Trim()
            .AgainstTooLong(500, "producer.description.max.length");

        Description = string.IsNullOrEmpty(description) ? null : description;
        if (addEvent) AddDomainEvent(new ProducerUpdatedDomainEvent(Id));
    }

    public void SetName(string name) => SetName(name, true);
    
    private void SetName(string name, bool addEvent)
    {
        var value = name.Trim();

        value.AgainstNullOrWhiteSpace("producer.name.not.empty")
            .AgainstTooShort(2, "producer.name.min.length")
            .AgainstTooLong(64, "producer.name.max.length");

        Name = ToNormalizedName(value);
        if (addEvent) AddDomainEvent(new ProducerUpdatedDomainEvent(Id));
    }

    public override void OnDeleted()
    {
        AddDomainEvent(new ProducerDeletedDomainEvent(Id));
    }

    public static string ToNormalizedName(string value) { return value.Trim().ToUpperInvariant(); }

    public override int GetId() { return Id; }
}