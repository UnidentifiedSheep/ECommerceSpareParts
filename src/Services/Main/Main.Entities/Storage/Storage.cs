using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Exceptions;
using Main.Enums;

namespace Main.Entities.Storage;

public class Storage : AuditableEntity<Storage, string>
{
    [Validate]
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? Location { get; private set; }

    public StorageType Type { get; private set; }

    private List<StorageOwner> _owners = [];
    public IReadOnlyCollection<StorageOwner> Owners => _owners;
    private Storage(){}

    private Storage(string name, StorageType type)
    {
        SetName(name);
        SetType(type);
    }

    public static Storage Create(string name, StorageType type)
    {
        return new Storage(name, type);
    }

    private void SetName(string name)
    {
        Name = name
            .Trim()
            .AgainstNullOrEmpty("storage.name.not.empty")
            .AgainstTooShort(6, "storage.name.min.length")
            .AgainstTooLong(128, "storage.name.max.length");
    }
    
    public void SetType(StorageType type)
    {
        if (Type == type) return;

        if (type == StorageType.SupplierStorage && _owners.Count != 0)
            throw new InvalidInputException("storage.type.change.restricted");
        
        Type = type;
    }

    public void SetDescription(string? description)
    {
        Description = description
            .NullIfWhiteSpace()?
            .AgainstTooLong(256, "storage.description.max.length");
    }
    
    public void SetLocation(string? location)
    {
        Location = location
            .NullIfWhiteSpace()?
            .AgainstTooLong(256, "storage.location.max.length");
    }

    public void AddOwner(Guid userId)
    {
        if (_owners.Any(x => x.OwnerId == userId)) return;
        _owners.Add(StorageOwner.Create(Name, userId));
    }

    public void RemoveOwner(Guid userId)
    {
        var found = _owners.FirstOrDefault(x => x.OwnerId == userId);
        if (found == null) return;
        _owners.Remove(found);
    }

    public override string GetId() => Name;
}