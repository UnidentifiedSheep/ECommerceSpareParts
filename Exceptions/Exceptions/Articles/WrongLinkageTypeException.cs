using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Articles;

public class WrongLinkageTypeException(string linkageType) : BadRequestException($"Неизвестный способ кроссировки.", new { LinkageType = linkageType })
{
    
}