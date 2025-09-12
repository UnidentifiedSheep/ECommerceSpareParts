using Application.Interfaces;

using AmwArticleDto = Core.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Core.Dtos.Anonymous.Articles.ArticleDto;

namespace Application.Handlers.Articles.GetArticles;

public class GetArticlesAmwLogSettings : ILoggableRequest<GetArticlesQuery<AmwArticleDto>>
{
    public bool IsLoggingNeeded(GetArticlesQuery<AmwArticleDto> request) => true;
    public string GetLogPlace(GetArticlesQuery<AmwArticleDto> request) => "Articles | Артикулы";
    public object GetLogData(GetArticlesQuery<AmwArticleDto> request) => request!;
    public string? GetUserId(GetArticlesQuery<AmwArticleDto> request) => request.UserId;
    
}

public class GetArticlesMemberLogSettings : ILoggableRequest<GetArticlesQuery<AnonymousArticleDto>>
{
    public bool IsLoggingNeeded(GetArticlesQuery<AnonymousArticleDto> request) => true;
    public string GetLogPlace(GetArticlesQuery<AnonymousArticleDto> request) => "Articles | Артикулы";
    public object GetLogData(GetArticlesQuery<AnonymousArticleDto> request) => request!;
    public string? GetUserId(GetArticlesQuery<AnonymousArticleDto> request) => request.UserId;
}