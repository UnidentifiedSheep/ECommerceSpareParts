using Application.Common.Interfaces;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public class GetArticleCrossesAmwLogSettings : ILoggableRequest<GetArticleCrossesQuery>
{
    public string GetLogPlace(GetArticleCrossesQuery request)
    {
        return "Article Crosses | Кросс номера артикула";
    }

    public object GetLogData(GetArticleCrossesQuery request)
    {
        return request;
    }

    public bool IsLoggingNeeded(GetArticleCrossesQuery request)
    {
        return request.UserId != null && request.UserId != Guid.Empty;
    }

    public Guid? GetUserId(GetArticleCrossesQuery request)
    {
        return request.UserId;
    }
}