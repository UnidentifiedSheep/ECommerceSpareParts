using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class CartItemNotFoundException(int articleId) : LocalizedNotFoundException(
	CartItemNotFoundMessage.Instance,
	new
	{
		ArticleId = articleId
	});

public class CartItemAlreadyExistsException(object values) : LocalizedConflictException(
	ItemAlreadyInCartMessage.Instance, values);
