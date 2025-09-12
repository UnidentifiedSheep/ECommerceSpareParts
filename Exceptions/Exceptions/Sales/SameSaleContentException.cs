using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Sales;

public class SameSaleContentException(int id) : BadRequestException($"В продажи не может быть две позиции с одинаковым.", new { Id = id })
{
    
}