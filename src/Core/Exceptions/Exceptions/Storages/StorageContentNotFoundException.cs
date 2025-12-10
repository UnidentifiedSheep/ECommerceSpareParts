using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class StorageContentNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false,1)]
    public StorageContentNotFoundException(int id) : base("Не удалось найти на складе позицию.", new { Id = id })
    {
    }
    
    [ExampleExceptionValues(true, 10, 23, 132)]

    public StorageContentNotFoundException(IEnumerable<int> ids) : base("Не удалось найти на складе позиции",
        new { Ids = ids })
    {
    }
}