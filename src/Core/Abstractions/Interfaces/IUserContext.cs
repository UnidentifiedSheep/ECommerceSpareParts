namespace Abstractions.Interfaces;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid? UserIdOrNull { get; }
    IReadOnlySet<string> Roles { get; }
    IReadOnlySet<string> Permissions { get; }
}