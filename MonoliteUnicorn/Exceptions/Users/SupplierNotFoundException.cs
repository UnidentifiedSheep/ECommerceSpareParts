using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users;

public class SupplierNotFoundException : NotFoundException
{
    public SupplierNotFoundException(string key) : base($"Не удалось найти поставщика {key}")
    {
    }
    public SupplierNotFoundException(IEnumerable<string> ids) : base($"Не удалось найти поставщиков {string.Join(',', ids)}")
    {
    }
}