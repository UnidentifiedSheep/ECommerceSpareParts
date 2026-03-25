using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Auth;

public class UserAlreadyContainsRoleException : ConflictException, ILocalizableException
{
    public string MessageKey => "user.already.have.this.role";
    public object[]? Arguments { get; }
    public UserAlreadyContainsRoleException(Guid userId, string role) 
        : base(null, new { UserId = userId, Role = role})
    {
        Arguments = [role];
    }
}