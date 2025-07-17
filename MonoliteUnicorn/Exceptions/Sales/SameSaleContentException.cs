using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Sales;

public class SameSaleContentException(int id) : BadRequestException($"В продажи не может быть две позиции с одинаковым.", new { Id = id })
{
    
}