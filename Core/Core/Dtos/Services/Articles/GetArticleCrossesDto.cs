namespace Core.Dtos.Services.Articles;

public record GetArticleCrossesDto(int ArticleId, int ViewCount, int Page, string? SortBy);