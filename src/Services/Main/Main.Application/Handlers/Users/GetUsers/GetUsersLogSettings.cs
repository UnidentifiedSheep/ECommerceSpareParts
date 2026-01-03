using Application.Common.Interfaces;

namespace Main.Application.Handlers.Users.GetUsers;

public class GetUsersLogSettings : ILoggableRequest<GetUsersQuery>
{
    bool ILoggableRequest<GetUsersQuery>.IsLoggingNeeded(GetUsersQuery request)
    {
        return request.WhoSearchedUserId != null && request.WhoSearchedUserId != Guid.Empty;
    }

    public string GetLogPlace(GetUsersQuery request)
    {
        return "Users | Пользователи";
    }

    public object GetLogData(GetUsersQuery request)
    {
        return request;
    }

    public Guid? GetUserId(GetUsersQuery request)
    {
        return request.WhoSearchedUserId;
    }
}