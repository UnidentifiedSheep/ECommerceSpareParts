using Exceptions.Base;

namespace Exceptions.Exceptions.StorageRoutes;

public class StorageRouteNotFound : NotFoundException
{
    public StorageRouteNotFound(string storageFrom, string storageTo)
        : base($"Маршрут между складом \"{storageFrom}\" и складом \"{storageTo}\" не найден.",
            new { StorageFrom = storageFrom, StorageTo = storageTo }) { }

    public StorageRouteNotFound(Guid id)
        : base("Маршрут складов с указанным идентификатором не найден.", new { Id = id }) { }
}