using Core.Dtos.Amw.Sales;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

public record RestoreContentItem(SaleContentDetailDto Detail, int ArticleId);