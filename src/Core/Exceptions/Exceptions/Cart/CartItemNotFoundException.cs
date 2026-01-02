using Exceptions.Base;

namespace Exceptions.Exceptions.Cart;

public class CartItemNotFoundException : NotFoundException
{
    public CartItemNotFoundException(int articleId) : base("Не удалось найти продукт в корзине.", new { ArticleId = articleId })
    {
    }
}