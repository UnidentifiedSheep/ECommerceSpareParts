using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageContentNotFoundException : NotFoundException
{
    public StorageContentNotFoundException(int id) : base($"Не удалось найти на складе позицию с id = {id}") { }

    public StorageContentNotFoundException(IEnumerable<int> ids) : base($"Не удалось найти на складе позиции ({string.Join(',', ids)})") { }
}