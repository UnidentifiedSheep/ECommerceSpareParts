using Application.Common.Interfaces;
using AmwArticleDto = Main.Core.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Main.Core.Dtos.Anonymous.Articles.ArticleDto;

namespace Main.Application.Handlers.Articles.GetArticles;

public class GetArticlesAmwLogSettings : ILoggableRequest<GetArticlesQuery<AmwArticleDto>>
{
    public bool IsLoggingNeeded(GetArticlesQuery<AmwArticleDto> request)
    {
        return true;
    }

    public string GetLogPlace(GetArticlesQuery<AmwArticleDto> request)
    {
        return "Articles | Артикулы";
    }

    public object GetLogData(GetArticlesQuery<AmwArticleDto> request)
    {
        return request;
    }

    public string? GetUserId(GetArticlesQuery<AmwArticleDto> request)
    {
        return request.UserId;
    }
}

public class GetArticlesMemberLogSettings : ILoggableRequest<GetArticlesQuery<AnonymousArticleDto>>
{
    public bool IsLoggingNeeded(GetArticlesQuery<AnonymousArticleDto> request)
    {
        return true;
    }

    public string GetLogPlace(GetArticlesQuery<AnonymousArticleDto> request)
    {
        return "Articles | Артикулы";
    }

    public object GetLogData(GetArticlesQuery<AnonymousArticleDto> request)
    {
        return request;
    }

    public string? GetUserId(GetArticlesQuery<AnonymousArticleDto> request)
    {
        return request.UserId;
    }
}