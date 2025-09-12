using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class StorageContentNotFoundException : NotFoundException
{
    public StorageContentNotFoundException(int id) : base("Не удалось найти на складе позицию.", new { Id = id })
    {
    }

    public StorageContentNotFoundException(IEnumerable<int> ids) : base("Не удалось найти на складе позиции",
        new { Ids = ids })
    {
    }
}