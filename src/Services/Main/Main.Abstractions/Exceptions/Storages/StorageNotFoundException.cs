using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Storages;

public class StorageNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "storage.not.found";
    public object[]? Arguments { get; }
    public StorageNotFoundException(string name) : base(null, new { Name = name })
    {
        Arguments = [name];
    }
}