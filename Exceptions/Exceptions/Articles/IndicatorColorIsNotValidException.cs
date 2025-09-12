using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Articles;

public class IndicatorColorIsNotValidException(string color) : BadRequestException($"Цвет '{color}' не валиден", new { Color = color })
{
    
}