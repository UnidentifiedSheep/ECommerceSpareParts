using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Articles;

public class ArticleContentCannotBeSameAsArticleException(int id) : 
    BadRequestException("Артикул не может быть частью самого себя.", new {Id = id})
{
    
}