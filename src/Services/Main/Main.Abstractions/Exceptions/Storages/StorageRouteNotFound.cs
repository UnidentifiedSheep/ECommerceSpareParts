using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Storages;

public class StorageRouteNotFound : NotFoundException, ILocalizableException
{
    public string MessageKey { get; }
    public object[]? Arguments { get; }

    public StorageRouteNotFound(string storageFrom, string storageTo)
        : base(null, new { StorageFrom = storageFrom, StorageTo = storageTo })
    {
        MessageKey = "storage.route.not.found.by.names";
        Arguments = [storageFrom, storageTo];
    }

    public StorageRouteNotFound(Guid id)
        : base("Маршрут складов с указанным идентификатором не найден.", new { Id = id })
    {
        MessageKey = "storage.route.not.found.by.id";
        Arguments = null;
    }

}