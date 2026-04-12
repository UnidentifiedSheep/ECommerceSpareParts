using Application.Common.Interfaces;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public class GetArticleCrossesAmwLogSettings : ILoggableRequest<GetProductCrossesQuery>
{
    public string GetLogPlace(GetProductCrossesQuery request)
    {
        return "Article Crosses | Кросс номера артикула";
    }

    public object GetLogData(GetProductCrossesQuery request)
    {
        return request;
    }

    public bool IsLoggingNeeded(GetProductCrossesQuery request)
    {
        return request.UserId != null && request.UserId != Guid.Empty;
    }

    public Guid? GetUserId(GetProductCrossesQuery request)
    {
        return request.UserId;
    }
}