using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Storages;

public class StorageOwnerNotFoundException : NotFoundException, ILocalizableException
{
    public StorageOwnerNotFoundException(Guid userId, string storageName)
        : base(null, new { UserId = userId, StorageName = storageName })
    {
        Arguments = [storageName];
    }

    public string MessageKey => "storage.not.found.in.user";
    public object[]? Arguments { get; }
}