using Application.Interfaces;

namespace Application.Handlers.Users.GetUsers;

public class GetUsersLogSettings : ILoggableRequest<GetUsersQuery>
{
    bool ILoggableRequest<GetUsersQuery>.IsLoggingNeeded(GetUsersQuery request)
        => !string.IsNullOrWhiteSpace(request.WhoSearchedUserId);

    public string GetLogPlace(GetUsersQuery request) => "Users | Пользователи";

    public object GetLogData(GetUsersQuery request) => request;
    public string? GetUserId(GetUsersQuery request) => request.WhoSearchedUserId;
}