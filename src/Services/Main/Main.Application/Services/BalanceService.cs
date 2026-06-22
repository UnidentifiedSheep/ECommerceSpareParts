using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Options;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Microsoft.Extensions.Options;

namespace Main.Application.Services;

public class BalanceService(
    IRepository<UserBalance, UserBalanceKey> userBalanceRepository,
    IRepository<UserFinancialProfile, Guid> userFinancialProfileRepository,
    ICurrencyConverter currencyConverter,
    IOptions<SystemOptions> systemOptions,
    IUnitOfWork unitOfWork) : IBalanceService
{
    public async Task ChangeSenderReceiverBalancesAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        var senderProfile = await GetFinancialProfile(transaction.SenderId, cancellationToken);
        var receiverProfile = await GetFinancialProfile(transaction.ReceiverId, cancellationToken);
        
        var senderBalance = await GetUserBalanceAsync(
            transaction.SenderId, 
            transaction.CurrencyId, 
            cancellationToken);
        var receiverBalance = await GetUserBalanceAsync(
            transaction.ReceiverId, 
            transaction.CurrencyId, 
            cancellationToken);
        
        var amountInBaseCurrency = await currencyConverter
            .ConvertToBaseAsync(
                transaction.Amount, 
                transaction.CurrencyId, 
                cancellationToken);

        transaction.Apply(
            senderBalance,
            receiverBalance,
            senderProfile,
            receiverProfile,
            amountInBaseCurrency,
            systemOptions.Value.SystemId);
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

    private async Task<UserFinancialProfile> GetFinancialProfile(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<UserFinancialProfile>.New()
            .Where(x => x.UserId == userId)
            .ForUpdate()
            .Track()
            .Build();

        var dbValue = await userFinancialProfileRepository
            .FirstOrDefaultAsync(
                criteria,
                cancellationToken);

        if (dbValue != null) return dbValue;

        dbValue = systemOptions.Value.SystemId == userId
            ? UserFinancialProfile.Create(userId, decimal.MinValue)
            : UserFinancialProfile.Create(userId);
        
        await unitOfWork.AddAsync(dbValue, cancellationToken);
        return dbValue;
    }
}
