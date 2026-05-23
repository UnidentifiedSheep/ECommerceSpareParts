using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions.Cart;

public class CartItemNotFoundException(int articleId)
    : LocalizedNotFoundException("cart.item.not.found", new { ArticleId = articleId });
