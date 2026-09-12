using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CartItemNotFoundException(int articleId) : LocalizedNotFoundException(
	CartItemNotFoundMessage.Instance,
	new
	{
		ArticleId = articleId
	});
