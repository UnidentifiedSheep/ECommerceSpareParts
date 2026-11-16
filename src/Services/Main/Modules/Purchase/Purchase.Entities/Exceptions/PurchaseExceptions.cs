using Exceptions.Base.Localized;

namespace Main.Entities.Purchase.Exceptions;

public class ArticleDoesntMatchContentException(int id) : LocalizedBadRequestException(
	ContentArticleDoesntMatchPurchasePositionMessage.Instance,
	new
	{
		Id = id
	});

public class PurchaseContentNotFoundException(int id) : LocalizedNotFoundException(
	PurchaseContentNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class PurchaseNotFoundException(Guid id) : LocalizedNotFoundException(
	PurchaseNotFoundMessage.Instance,
	new
	{
		Id = id
	});
