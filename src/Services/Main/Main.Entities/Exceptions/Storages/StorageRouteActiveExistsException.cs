using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Entities.Exceptions.Storages;

public class StorageRouteActiveExistsException : ConflictException, ILocalizableException
{
    public StorageRouteActiveExistsException(string from, string to)
        : base(null, new { From = from, To = to })
    {
        Arguments = [from, to];
    }

    public string MessageKey => "active.storage.route.exists";
    public object[]? Arguments { get; }
}