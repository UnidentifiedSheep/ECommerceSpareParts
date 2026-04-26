using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Entities.User;

namespace Main.Application.Services;

public class BalanceService(
    IRepository<UserBalance, UserBalanceKey> userBalanceRepository,
    IUnitOfWork unitOfWork) : IBalanceService
{
    public async Task ChangeSenderReceiverBalancesAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        var senderBalance = await GetUserBalanceAsync(transaction.SenderId, transaction.CurrencyId, cancellationToken);
        var receiverBalance = await GetUserBalanceAsync(transaction.ReceiverId, transaction.CurrencyId, cancellationToken);

        transaction.Apply(senderBalance, receiverBalance);
    }

    private async Task<UserBalance> GetUserBalanceAsync(
        Guid userId,
        int currencyId,
        CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<UserBalance>.New()
            .Where(x => x.UserId == userId && x.CurrencyId == currencyId)
            .ForUpdate()
            .Track()
            .Build();

        var dbValue = await userBalanceRepository.FirstOrDefaultAsync(criteria, cancellationToken);

        if (dbValue != null) return dbValue;
        
        dbValue = UserBalance.Create(userId, currencyId);
        await unitOfWork.AddAsync(dbValue, cancellationToken);

        return dbValue;
    }
}