using Exceptions.Base;

namespace Exceptions.Exceptions.StorageOwner;

public class StorageOwnerNotFoundException : NotFoundException
{
    public StorageOwnerNotFoundException(Guid userId, string storageName) 
        : base("Не удалось найти склад во владениях у пользователя.", 
            new { UserId = userId, StorageName = storageName }) {}
}