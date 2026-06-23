namespace Internal.Integration.Core.Interfaces.Main;

public interface IUserNode
{
    Task<decimal> GetUserDiscount(Guid userId, CancellationToken cancellationToken = default);
}