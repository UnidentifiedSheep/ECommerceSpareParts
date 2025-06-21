using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users;

public class SupplierNotFoundException(string key) : NotFoundException($"Не удалось найти поставщика {key}")
{
    
}