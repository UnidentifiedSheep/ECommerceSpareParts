using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;

namespace Main.Entities.Storage;

public class StorageOwner : AuditableEntity<StorageOwner, (string, Guid)>, ILinqEntity<StorageOwner, (string, Guid)>
{
    private StorageOwner()
    {
    }

    private StorageOwner(string storageName, Guid userId)
    {
        StorageName = storageName;
        UserId = userId;
    }

    [ValidateTuple("PK")]
    [Validate]
    public string StorageName { get; } = null!;

    [ValidateTuple("PK")]
    public Guid UserId { get; }

    public User.User User { get; private set; } = null!;

    public Storage Storage { get; private set; } = null!;

    public static Expression<Func<StorageOwner, bool>> GetEqualityExpression((string, Guid) key)
    {
        return x => x.StorageName == key.Item1 && x.UserId == key.Item2;
    }

    public static StorageOwner Create(string storageName, Guid ownerId)
    {
        return new StorageOwner(storageName, ownerId);
    }

    public override (string, Guid) GetId()
    {
        return (StorageName, UserId);
    }
}