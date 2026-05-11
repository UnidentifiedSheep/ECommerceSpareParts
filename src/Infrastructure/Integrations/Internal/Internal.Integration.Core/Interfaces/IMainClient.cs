namespace Internal.Integration.Core.Interfaces;

public interface IMainClient
{
    Task<decimal> GetUserDiscount(Guid userId, CancellationToken cancellationToken = default);
}