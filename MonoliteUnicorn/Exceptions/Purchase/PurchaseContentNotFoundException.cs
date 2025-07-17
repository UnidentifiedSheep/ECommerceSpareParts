using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class PurchaseContentNotFoundException(int id) : NotFoundException($"Не удалось найти позицию закупки", new { Id = id })
{
    
}