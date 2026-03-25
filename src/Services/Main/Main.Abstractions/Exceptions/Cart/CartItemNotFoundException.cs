using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Cart;

public class CartItemNotFoundException : NotFoundException, ILocalizableException
{
    public string MessageKey => "cart.item.not.found";
    public object[]? Arguments => null;
    public CartItemNotFoundException(int articleId) : base(null, new { ArticleId = articleId })
    {
    }

}