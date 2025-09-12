using Exceptions.Base;

namespace Exceptions.Exceptions.Purchase;

public class PurchaseNotFoundException(string id) : NotFoundException($"Не удалось найти закупку", new { Id = id })
{
    
}