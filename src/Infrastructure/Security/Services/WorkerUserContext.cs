using Abstractions.Interfaces;
using Abstractions.Models.Options;
using Microsoft.Extensions.Options;

namespace Security.Services;

public class WorkerUserContext(
    IOptions<SystemOptions> options
) : IUserContext
{
    public bool IsAuthenticated => true;
    public Guid UserId => options.Value.SystemId;
    public Guid? UserIdOrNull => options.Value.SystemId;
    public IReadOnlySet<string> Roles => new HashSet<string>();
    public IReadOnlySet<string> Permissions => new HashSet<string>();
}