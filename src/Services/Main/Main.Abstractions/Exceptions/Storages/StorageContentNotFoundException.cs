using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Storages;

public class StorageContentNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "storage.content.not.found";
    public object[]? Arguments => null;
    public StorageContentNotFoundException(int id) 
        : base(null, new { Id = id }) { }
    
   
    public StorageContentNotFoundException(IEnumerable<int> ids) 
        : base(null, new { Ids = ids }) { }
}