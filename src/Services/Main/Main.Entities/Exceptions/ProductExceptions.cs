using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class ProductCharacteristicsNotFoundException(int id, string name) : LocalizedNotFoundException(
	ArticleCharacteristicsNotFoundMessage.Instance,
	new
	{
		Id = id, Name = name
	});

public class ProductContentNotFoundException(int articleId, int insideArticleId) : LocalizedNotFoundException(
	ArticleContentNotFoundMessage.Instance,
	new
	{
		MainArticleId = articleId, InsideArticleId = insideArticleId
	});

public class ProductImageNotFoundException(int productId, string path) : LocalizedNotFoundException(
	ArticleImageNotFoundMessage.Instance,
	new
	{
		ProductId = productId, ImagePath = path
	});

public class ProductNotFoundException : LocalizedNotFoundException
{
	public ProductNotFoundException(int id) : base(
		ArticleNotFoundMessage.Instance,
		new
		{
			Id = id
		})
	{
	}

	public ProductNotFoundException(IEnumerable<int> ids) : base(
		ArticlesNotFoundMessage.Instance,
		new
		{
			Ids = ids
		})
	{
	}
}

public class ProductSizesNotFoundException(int articleId) : LocalizedNotFoundException(
	ArticleSizesNotFoundMessage.Instance,
	new
	{
		ArticleId = articleId
	});

public class ProductWeightNotFoundException(int articleId) : LocalizedNotFoundException(
	ArticleWeightNotFoundMessage.Instance,
	new
	{
		ArticleId = articleId
	});

public class ReservationNotFoundException(int id) : LocalizedNotFoundException(
	ArticleReservationNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class ProductCrossSelfReferenceException()
	: LocalizedBadRequestException(ArticleLinkageArticleCannotEqualCrossArticleMessage.Instance);

