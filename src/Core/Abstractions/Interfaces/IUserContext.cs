namespace Abstractions.Interfaces;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    IReadOnlySet<string> Roles { get; }
    IReadOnlySet<string> Permissions { get; }
}