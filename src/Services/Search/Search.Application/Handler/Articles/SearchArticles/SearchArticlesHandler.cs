using Application.Common.Interfaces;
using Search.Abstractions.Dtos;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;
using Search.Entities;
using Search.Enums;

namespace Search.Application.Handler.Articles.SearchArticles;

public record SearchArticlesQuery(string Query, string? Cursor, int Limit, ArticleSearchVariant SearchVariant)
    : IQuery<SearchArticlesResult>;

public record SearchArticlesResult(IReadOnlyList<ArticleDto> Articles, string? Cursor);

public class SearchArticlesHandler(IArticleReadService readService)
    : IQueryHandler<SearchArticlesQuery, SearchArticlesResult>
{
    public Task<SearchArticlesResult> Handle(SearchArticlesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Article> articles;
        string? newCursor;
        switch (query.SearchVariant)
        {
            case ArticleSearchVariant.ByTitle:
                (articles, newCursor) = readService.SearchByTitle(query.Query, query.Cursor, query.Limit);
                break;
            case ArticleSearchVariant.ByArticleNumberPrefix:
                (articles, newCursor) = readService.SearchByArticleNumberPrefix(query.Query, query.Cursor, query.Limit);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.FromResult(new SearchArticlesResult(articles.ToDtos(), newCursor));
    }
}