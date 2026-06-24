using Internal.Integration.Core.Models;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IUserNode
{
    Task<InternalResponse<decimal>> GetUserDiscount(Guid userId, CancellationToken cancellationToken = default);
}