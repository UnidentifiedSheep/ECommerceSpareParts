using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Auth;

public class PermissionNotFoundException : NotFoundException, ILocalizableException
{
    public PermissionNotFoundException(string name) : base(null, new { Name = name })
    {
        Arguments = [name];
    }

    public string MessageKey => "permission.not.found";
    public object[]? Arguments { get; }
}