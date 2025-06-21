using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class PurchaseNotFoundException(string purchaseId) : NotFoundException($"Не удалось найти закупку '{purchaseId}'")
{
    
}