using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles;

public class WrongLinkageTypeException(string linkageType) : BadRequestException($"Неизвестный способ кроссировки '{linkageType}'")
{
    
}