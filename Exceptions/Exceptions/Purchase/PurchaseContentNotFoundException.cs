using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Purchase;

public class PurchaseContentNotFoundException(int id) : NotFoundException($"Не удалось найти позицию закупки", new { Id = id })
{
    
}