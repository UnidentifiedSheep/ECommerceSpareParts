using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Purchase;

public class PurchaseContentNotFoundException(int? contentId) : NotFoundException($"Не удалось найти позицию закупки, с айди {contentId}")
{
    
}