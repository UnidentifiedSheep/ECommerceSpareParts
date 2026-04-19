using BulkValidation.Core.Attributes;
using Domain;

namespace Main.Entities.Storage;

public class StorageOwner : AuditableEntity<StorageOwner, (string, Guid)>
{
    [ValidateTuple("PK")]
    [Validate]
    public string StorageName { get; private set; } = null!;

    [ValidateTuple("PK")]
    public Guid UserId { get; private set; }

    public User.User User { get; private set; } = null!;

    public Storage Storage { get; private set; } = null!;

    private StorageOwner() { }

    private StorageOwner(string storageName, Guid userId)
    {
        StorageName = storageName;
        UserId = userId;
    }
    
    public static StorageOwner Create(string storageName, Guid ownerId)
    {
        return new StorageOwner(storageName, ownerId);
    }
    
    public override (string, Guid) GetId() => (StorageName, UserId);
}