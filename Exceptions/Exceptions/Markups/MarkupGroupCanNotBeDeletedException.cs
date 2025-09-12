using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Markups;

public class MarkupGroupCanNotBeDeletedException() : BadRequestException("Нельзя удалить используемую политику наценок")
{
    
}