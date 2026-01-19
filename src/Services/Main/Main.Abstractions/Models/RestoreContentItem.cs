using Main.Abstractions.Dtos.Amw.Sales;

namespace Main.Abstractions.Models;

public record RestoreContentItem(SaleContentDetailDto Detail, int ArticleId);