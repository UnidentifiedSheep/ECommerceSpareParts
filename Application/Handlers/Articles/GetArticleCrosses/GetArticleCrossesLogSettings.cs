using AmwArticleDto = Core.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = Core.Dtos.Member.Articles.ArticleFullDto;

using Application.Interfaces;

namespace Application.Handlers.Articles.GetArticleCrosses;

public class GetArticleCrossesAmwLogSettings : ILoggableRequest<GetArticleCrossesQuery<AmwArticleDto>>
{
    
    public string GetLogPlace(GetArticleCrossesQuery<AmwArticleDto> request) => "Article Crosses | Кросс номера артикула";
    public object GetLogData(GetArticleCrossesQuery<AmwArticleDto> request) => request;
    public bool IsLoggingNeeded(GetArticleCrossesQuery<AmwArticleDto> request)
        => !string.IsNullOrWhiteSpace(request.UserId);
    
    public string? GetUserId(GetArticleCrossesQuery<AmwArticleDto> request) => request.UserId;
}

public class GetArticleCrossesMemberLogSettings : ILoggableRequest<GetArticleCrossesQuery<MemberArticleDto>>
{
    public string GetLogPlace(GetArticleCrossesQuery<MemberArticleDto> request) => "Article Crosses | Кросс номера артикула";
    public object GetLogData(GetArticleCrossesQuery<MemberArticleDto> request) => request;
    public bool IsLoggingNeeded(GetArticleCrossesQuery<MemberArticleDto> request)
        => !string.IsNullOrWhiteSpace(request.UserId);
    
    public string? GetUserId(GetArticleCrossesQuery<MemberArticleDto> request) => request.UserId;
}
