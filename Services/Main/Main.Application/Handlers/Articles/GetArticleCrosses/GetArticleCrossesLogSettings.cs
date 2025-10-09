using Application.Common.Interfaces;
using AmwArticleDto = Core.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = Core.Dtos.Member.Articles.ArticleFullDto;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public class GetArticleCrossesAmwLogSettings : ILoggableRequest<GetArticleCrossesQuery<AmwArticleDto>>
{
    public string GetLogPlace(GetArticleCrossesQuery<AmwArticleDto> request)
    {
        return "Article Crosses | Кросс номера артикула";
    }

    public object GetLogData(GetArticleCrossesQuery<AmwArticleDto> request)
    {
        return request;
    }

    public bool IsLoggingNeeded(GetArticleCrossesQuery<AmwArticleDto> request)
    {
        return !string.IsNullOrWhiteSpace(request.UserId);
    }

    public string? GetUserId(GetArticleCrossesQuery<AmwArticleDto> request)
    {
        return request.UserId;
    }
}

public class GetArticleCrossesMemberLogSettings : ILoggableRequest<GetArticleCrossesQuery<MemberArticleDto>>
{
    public string GetLogPlace(GetArticleCrossesQuery<MemberArticleDto> request)
    {
        return "Article Crosses | Кросс номера артикула";
    }

    public object GetLogData(GetArticleCrossesQuery<MemberArticleDto> request)
    {
        return request;
    }

    public bool IsLoggingNeeded(GetArticleCrossesQuery<MemberArticleDto> request)
    {
        return !string.IsNullOrWhiteSpace(request.UserId);
    }

    public string? GetUserId(GetArticleCrossesQuery<MemberArticleDto> request)
    {
        return request.UserId;
    }
}