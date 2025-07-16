using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles;

public class ArticleContentCountCannotBeNegative() : BadRequestException("Количество содержимого не может быть меньше нуля.")
{
    
}