using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles;

public class IndicatorColorIsNotValidException(string color) : BadRequestException($"Цвет '{color}' не валиден", new { Color = color })
{
    
}