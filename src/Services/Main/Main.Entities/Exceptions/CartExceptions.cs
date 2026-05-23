using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CartItemNotFoundException(int articleId)
    : LocalizedNotFoundException("cart.item.not.found", new { ArticleId = articleId });
