using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Markups;

public class MarkupGroupCanNotBeDeletedException() : BadRequestException("Нельзя удалить используемую политику наценок")
{
    
}