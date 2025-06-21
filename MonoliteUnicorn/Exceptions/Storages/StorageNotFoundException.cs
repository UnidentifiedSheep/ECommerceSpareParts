using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Storages;

public class StorageNotFoundException : NotFoundException
{
    public StorageNotFoundException(string key) : base($"Склад не найден {key}") { }

    public StorageNotFoundException(IEnumerable<string> names) : base($"Не удалось найти склады ({string.Join(',', names)})") { }
}