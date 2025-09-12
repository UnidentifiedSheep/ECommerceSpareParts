using Application.Interfaces;
using Core.Entities;
using Core.StaticFunctions;
using AmwArticleDto = Core.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = Core.Dtos.Member.Articles.ArticleFullDto;

namespace Application.Handlers.Articles.GetArticleCrosses;

public class GetArticleCrossesAmwCacheSettings : ICacheableQuery<GetArticleCrossesQuery<AmwArticleDto>>
{
    public string GetCacheKey(GetArticleCrossesQuery<AmwArticleDto> request) 
        => string.Format(CacheKeys.ArticleCrossesCacheKey, request.ArticleId, request.Pagination.Page, request.Pagination.Size, request.SortBy);

    public string GetEntityId(GetArticleCrossesQuery<AmwArticleDto> request) => request.ArticleId.ToString();

    public Type GetRelatedType(GetArticleCrossesQuery<AmwArticleDto> request) => typeof(Article);

    public int GetDurationSeconds(GetArticleCrossesQuery<AmwArticleDto> request) => 600;
}

public class GetArticleCrossesMemberCacheSettings : ICacheableQuery<GetArticleCrossesQuery<MemberArticleDto>>
{
    public string GetCacheKey(GetArticleCrossesQuery<MemberArticleDto> request) 
        => string.Format(CacheKeys.ArticleCrossesCacheKey, request.ArticleId, request.Pagination.Page, request.Pagination.Size, request.SortBy);

    public string GetEntityId(GetArticleCrossesQuery<MemberArticleDto> request) => request.ArticleId.ToString();

    public Type GetRelatedType(GetArticleCrossesQuery<MemberArticleDto> request) => typeof(Article);

    public int GetDurationSeconds(GetArticleCrossesQuery<MemberArticleDto> request) => 600;
}