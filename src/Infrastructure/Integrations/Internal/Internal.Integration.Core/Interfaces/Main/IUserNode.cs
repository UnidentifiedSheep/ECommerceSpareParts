using Integrations.Common;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IUserNode
{
    Task<Response<decimal>> GetUserDiscount(Guid userId, CancellationToken cancellationToken = default);
}