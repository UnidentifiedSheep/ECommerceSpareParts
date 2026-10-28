using Exceptions.Base.Localized;
using Main.Enums.Products;

namespace Main.Entities.Exceptions;

public class ProductCharacteristicsNotFoundException(int id, int characteristicId) : LocalizedNotFoundException(
	ArticleCharacteristicsNotFoundMessage.Instance,
	new
	{
		Id = id, CharacteristicId = characteristicId
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

public class ProductEanNotFoundException(int productId, string ean) : LocalizedNotFoundException(
	ArticleEanNotFoundMessage.Instance,
	new { ProductId = productId, Ean = ean });

public class ProductEanAlreadyExistsException(int productId, string ean) : LocalizedConflictException(
	ArticleEanAlreadyExistsMessage.Instance,
	new { ProductId = productId, Ean = ean });

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

public class ProductGroupNotFoundException(int id) : LocalizedNotFoundException(
	ProductGroupNotFoundMessage.Instance,
	new { Id = id });

public class ProductGroupNameAlreadyExistsException() : LocalizedConflictException(
	ProductGroupNormalizedNameAlreadyExistsMessage.Instance);

public class ProductDeletionNotAllowedException(ProductDeletionBlocker blocker) : LocalizedConflictException(
	blocker switch
	{
		ProductDeletionBlocker.Stock => ArticleDeletionStockMessage.Instance,
		ProductDeletionBlocker.StorageContent => ArticleDeletionStorageContentMessage.Instance,
		ProductDeletionBlocker.Sale => ArticleDeletionSaleMessage.Instance,
		ProductDeletionBlocker.Purchase => ArticleDeletionPurchaseMessage.Instance,
		ProductDeletionBlocker.Order => ArticleDeletionOrderMessage.Instance,
		ProductDeletionBlocker.Reservation => ArticleDeletionReservationMessage.Instance,
		ProductDeletionBlocker.Composition => ArticleDeletionCompositionMessage.Instance,
		_ => throw new ArgumentOutOfRangeException(nameof(blocker))
	})
{
	public ProductDeletionBlocker Blocker { get; } = blocker;
}
