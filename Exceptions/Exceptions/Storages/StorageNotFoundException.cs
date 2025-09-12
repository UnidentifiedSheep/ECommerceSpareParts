using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class StorageNotFoundException : NotFoundException
{
    public StorageNotFoundException(string name) : base("Склад не найден", new { Name = name })
    {
    }

    public StorageNotFoundException(IEnumerable<string> names) : base("Не удалось найти склады", new { Names = names })
    {
    }
}