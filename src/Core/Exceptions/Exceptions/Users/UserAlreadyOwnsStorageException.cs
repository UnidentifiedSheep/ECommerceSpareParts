using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class UserAlreadyOwnsStorageException : ConflictException
{
    public UserAlreadyOwnsStorageException(Guid userId, string storageName) 
        : base("Склад уже привязан к данному пользователю", new { UserId = userId, StorageName = storageName }) { }
}