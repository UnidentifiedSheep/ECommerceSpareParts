using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles;

public class ArticleContentCannotBeSameAsArticleException(int articleId) : BadRequestException($"Артикул не может быть частью самого себя. Id = '{articleId}'.")
{
    
}