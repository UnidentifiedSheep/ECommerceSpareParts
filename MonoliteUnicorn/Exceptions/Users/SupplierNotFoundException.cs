using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users;

public class SupplierNotFoundException : NotFoundException
{
    public SupplierNotFoundException(string id) : base($"Не удалось найти поставщика", new { Id = id })
    {
    }
    public SupplierNotFoundException(IEnumerable<string> ids) : base($"Не удалось найти поставщиков", new { Ids = ids })
    {
    }
}