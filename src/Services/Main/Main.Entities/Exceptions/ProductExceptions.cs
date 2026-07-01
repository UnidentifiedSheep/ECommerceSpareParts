using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class ProductCharacteristicsNotFoundException(int id, string name)
    : LocalizedNotFoundException("article.characteristics.not.found", new { Id = id, Name = name });

public class ProductContentNotFoundException(int articleId, int insideArticleId)
    : LocalizedNotFoundException(
        "article.content.not.found",
        new { MainArticleId = articleId, InsideArticleId = insideArticleId });

public class ProductImageNotFoundException(int productId, string path)
    : LocalizedNotFoundException(
        "article.image.not.found",
        new { ProductId = productId, ImagePath = path });

public class ProductNotFoundException : LocalizedNotFoundException
{
    public ProductNotFoundException(int id)
        : base("article.not.found", new { Id = id })
    {
    }

    public ProductNotFoundException(IEnumerable<int> ids)
        : base("articles.not.found", new { Ids = ids })
    {
    }
}

public class ProductSizesNotFoundException(int articleId)
    : LocalizedNotFoundException("article.sizes.not.found", new { ArticleId = articleId });

public class ProductWeightNotFoundException(int articleId)
    : LocalizedNotFoundException("article.weight.not.found", new { ArticleId = articleId });

public class ReservationNotFoundException(int id)
    : LocalizedNotFoundException("article.reservation.not.found", new { Id = id });