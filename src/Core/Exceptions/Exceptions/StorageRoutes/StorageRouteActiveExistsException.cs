using Exceptions.Base;

namespace Exceptions.Exceptions.StorageRoutes;

public class StorageRouteActiveExistsException : ConflictException
{
    public StorageRouteActiveExistsException(string from, string to) 
        : base("Уже есть активный маршрут для таких складов.", new { From = from, To = to }) { }
}