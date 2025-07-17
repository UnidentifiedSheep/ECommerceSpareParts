using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class PurchaseNotFoundException(string id) : NotFoundException($"Не удалось найти закупку", new { Id = id })
{
    
}