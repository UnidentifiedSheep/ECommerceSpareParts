using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions;

public class ArticleDoesntMatchContentException(int id)
    : LocalizedBadRequestException(
        "content.article.doesnt.match.purchase.position",
        new { Id = id });

public class PurchaseContentNotFoundException(int id)
    : LocalizedNotFoundException("purchase.content.not.found", new { Id = id });

public class PurchaseLogisticNotFoundException(string purchaseId)
    : LocalizedNotFoundException(
        "purchase.logistics.data.not.found",
        new { PurchaseId = purchaseId });

public class PurchaseNotFoundException(Guid id)
    : LocalizedNotFoundException("purchase.not.found", new { Id = id });
