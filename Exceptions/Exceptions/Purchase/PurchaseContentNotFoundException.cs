using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class PurchaseContentNotFoundException(int id) : NotFoundException($"Не удалось найти позицию закупки", new { Id = id })
{
    
}