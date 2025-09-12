using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Articles;

public class ArticleContentCountCannotBeNegative() : BadRequestException("Количество содержимого не может быть меньше нуля.")
{
    
}