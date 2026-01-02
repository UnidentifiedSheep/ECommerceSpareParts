using Exceptions.Base;

namespace Exceptions.Exceptions.Cart;

public class SameItemInCartException : ConflictException
{
    public SameItemInCartException(int articleId) : base("Данный продукт уже есть в корзине.", new { ArticleId = articleId })
    {
    }
}