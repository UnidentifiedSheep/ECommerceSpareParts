using Abstractions.Interfaces.Persistence;
using Abstractions.Models.Options;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Entities.Organization;
using Microsoft.Extensions.Options;

namespace Main.Application.Services;

public class BalanceService(
    IRepository<OrganizationBalance, UserBalanceKey> userBalanceRepository,
    IRepository<OrganizationFinancialProfile, Guid> userFinancialProfileRepository,
    ICurrencyConverter currencyConverter,
    IOptions<SystemOptions> systemOptions,
    ITransactionFinancialProfileService transactionFinancialProfileService,
    IUnitOfWork unitOfWork
) : IBalanceService
{
    public async Task ChangeSenderReceiverBalancesAsync(
        Transaction transaction,
        bool forceFinancialProfileDebit = false,
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

        transaction.Apply(senderBalance, receiverBalance);
        transactionFinancialProfileService.Apply(
            transaction,
            senderProfile,
            receiverProfile,
            amountInBaseCurrency,
            systemOptions.Value.SystemId,
            forceFinancialProfileDebit);
    }

    private async Task<OrganizationBalance> GetUserBalanceAsync(
        Guid userId,
        int currencyId,
        CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<OrganizationBalance>.New()
            .Where(x => x.OrganizationId == userId && x.CurrencyId == currencyId)
            .ForUpdate()
            .Track()
            .Build();

        var dbValue = await userBalanceRepository.FirstOrDefaultAsync(criteria, cancellationToken);

        if (dbValue != null) return dbValue;

        dbValue = OrganizationBalance.Create(userId, currencyId);
        await unitOfWork.AddAsync(dbValue, cancellationToken);

        return dbValue;
    }

    private async Task<OrganizationFinancialProfile> GetFinancialProfile(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<OrganizationFinancialProfile>.New()
            .Where(x => x.OrganizationId == userId)
            .ForUpdate()
            .Track()
            .Build();

        var dbValue = await userFinancialProfileRepository
            .FirstOrDefaultAsync(
                criteria,
                cancellationToken);

        if (dbValue != null) return dbValue;

        dbValue = systemOptions.Value.SystemId == userId
            ? OrganizationFinancialProfile.Create(userId, decimal.MinValue)
            : OrganizationFinancialProfile.Create(userId);

        await unitOfWork.AddAsync(dbValue, cancellationToken);
        return dbValue;
    }
}