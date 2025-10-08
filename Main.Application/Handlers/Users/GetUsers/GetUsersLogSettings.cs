using Application.Common.Interfaces;

namespace Main.Application.Handlers.Users.GetUsers;

public class GetUsersLogSettings : ILoggableRequest<GetUsersQuery>
{
    bool ILoggableRequest<GetUsersQuery>.IsLoggingNeeded(GetUsersQuery request)
    {
        return !string.IsNullOrWhiteSpace(request.WhoSearchedUserId);
    }

    public string GetLogPlace(GetUsersQuery request)
    {
        return "Users | Пользователи";
    }

    public object GetLogData(GetUsersQuery request)
    {
        return request;
    }

    public string? GetUserId(GetUsersQuery request)
    {
        return request.WhoSearchedUserId;
    }
}