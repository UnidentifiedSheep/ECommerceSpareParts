using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class ChangeOfStorageTypeRestrictedException()
    : LocalizedBadRequestException("storage.type.change.restricted");

public class NotEnoughCountOnStorageException : LocalizedBadRequestException
{
    public NotEnoughCountOnStorageException(
        int articleId,
        int availableCount,
        int neededCount)
        : base(
            "not.enough.count.on.storage.for.article",
            new { ArticleId = articleId, AvailableCount = availableCount },
            [articleId, availableCount, neededCount])
    {
    }

    public NotEnoughCountOnStorageException(IEnumerable<int> ids)
        : base("not.enough.count.on.storage.for.articles", new { Ids = ids })
    {
    }
}

public class StorageContentNotFoundException : LocalizedNotFoundException
{
    public StorageContentNotFoundException(int id)
        : base("storage.content.not.found", new { Id = id })
    {
    }

    public StorageContentNotFoundException(IEnumerable<int> ids)
        : base("storage.content.not.found", new { Ids = ids })
    {
    }
}

public class StorageNotFoundException(string name)
    : LocalizedNotFoundException(
        "storage.not.found",
        new { Name = name },
        [name]);

public class StorageOwnerNotFoundException(Guid userId, string storageName)
    : LocalizedNotFoundException(
        "storage.not.found.in.user",
        new { UserId = userId, StorageName = storageName },
        [storageName]);

public class StorageRouteActiveExistsException(string from, string to)
    : LocalizedConflictException(
        "active.storage.route.exists",
        new { From = from, To = to },
        [from, to]);

public class StorageRouteNotFound : LocalizedNotFoundException
{
    public StorageRouteNotFound(string storageFrom, string storageTo)
        : base(
            "storage.route.not.found.by.names",
            new { StorageFrom = storageFrom, StorageTo = storageTo },
            [storageFrom, storageTo])
    {
    }

    public StorageRouteNotFound(Guid id)
        : base("storage.route.not.found.by.id", new { Id = id })
    {
    }
}