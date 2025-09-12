using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Purchase;

public class PurchaseNotFoundException(string id) : NotFoundException($"Не удалось найти закупку", new { Id = id })
{
    
}