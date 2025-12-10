using Core.Attributes;
using Core.Interfaces.Exceptions;
using Exceptions.Base;

namespace Exceptions.Exceptions.Roles;

public class RoleNotFoundException : NotFoundException
{
    [ExampleExceptionValues(false, typeof(RoleNotFoundGuidExample))]
    public RoleNotFoundException(Guid id) : base("Не удалось найти роль", new { Id = id })
    {
    }

    [ExampleExceptionValues(false, typeof(RoleNotFoundGuidsExample))]
    public RoleNotFoundException(IEnumerable<Guid> ids) : base("Не удалось найти роли", new { Ids = ids })
    {
    }
    
    [ExampleExceptionValues(false, typeof(RoleNotFoundNameExample))]
    public RoleNotFoundException(string roleName) : base("Не удалось найти роль", new { Name = roleName })
    {
    }

    [ExampleExceptionValues(false, typeof(RoleNotFoundNamesExample))]
    public RoleNotFoundException(IEnumerable<string> rolesNames) : base("Не удалось найти роли",
        new { Names = rolesNames })
    {
    }
}

public class RoleNotFoundGuidExample : IExceptionExample
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

public class RoleNotFoundGuidsExample : IExceptionExample
{
    public IEnumerable<Guid> Ids { get; set; } = [ Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
}

public class RoleNotFoundNameExample : IExceptionExample
{
    public string RoleName { get; set; } = "Example_Role";
}

public class RoleNotFoundNamesExample : IExceptionExample
{
    public IEnumerable<string> RoleName { get; set; } = ["Example_Role1", "Example_Role2"];
}