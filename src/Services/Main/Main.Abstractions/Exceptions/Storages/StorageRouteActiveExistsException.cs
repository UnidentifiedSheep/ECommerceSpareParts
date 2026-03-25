using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Storages;

public class StorageRouteActiveExistsException : ConflictException, ILocalizableException
{
    public string MessageKey => "active.storage.route.exists";
    public object[]? Arguments { get; }

    public StorageRouteActiveExistsException(string from, string to)
        : base(null, new { From = from, To = to })
    {
        Arguments = [from, to];
    }
}