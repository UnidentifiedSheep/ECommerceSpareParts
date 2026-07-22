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
    IRepository<OrganizationFinancialProfile, Guid> organizationFinancialProfileRepository,
    IRepository<Organization, Guid> organizationRepository,
    ICurrencyConverter currencyConverter,
    IOptions<SystemOptions> systemOptions,
    ITransactionFinancialProfileService transactionFinancialProfileService,
    IUnitOfWork unitOfWork
) : IBalanceService
{
    public async Task<decimal> GetBalanceInBaseCurrencyAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<OrganizationBalance>.New()
            .Where(x => x.OrganizationId == organizationId)
            .Build();
        var balances = await userBalanceRepository.ListAsync(criteria, cancellationToken);

        return await SumBalanceInBaseCurrencyAsync(balances, cancellationToken);
    }

    public async Task ChangeSenderReceiverBalancesAsync(
        Transaction transaction,
        bool forceFinancialProfileDebit = false,
        CancellationToken cancellationToken = default)
    {
        await LockOrganizationsAsync(
            transaction.SenderId,
            transaction.ReceiverId,
            cancellationToken);
        var profiles = await GetFinancialProfiles(
            transaction.SenderId,
            transaction.ReceiverId,
            cancellationToken);
        var senderProfile = profiles[transaction.SenderId];
        var receiverProfile = profiles[transaction.ReceiverId];

        var balances = await GetBalancesAsync(
            transaction.SenderId,
            transaction.ReceiverId,
            cancellationToken);
        var senderBalance = await GetOrCreateBalanceAsync(
            balances,
            transaction.SenderId,
            transaction.CurrencyId,
            cancellationToken);
        var receiverBalance = await GetOrCreateBalanceAsync(
            balances,
            transaction.ReceiverId,
            transaction.CurrencyId,
            cancellationToken);

        var senderBalanceInBaseCurrency = await SumBalanceInBaseCurrencyAsync(
            balances.Where(x => x.OrganizationId == transaction.SenderId),
            cancellationToken);
        var receiverBalanceInBaseCurrency = await SumBalanceInBaseCurrencyAsync(
            balances.Where(x => x.OrganizationId == transaction.ReceiverId),
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
            senderBalanceInBaseCurrency,
            receiverBalanceInBaseCurrency,
            amountInBaseCurrency,
            forceFinancialProfileDebit);
    }

    private async Task LockOrganizationsAsync(
        Guid senderId,
        Guid receiverId,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<Organization>.New()
            .Where(x => x.Id == senderId || x.Id == receiverId)
            .OrderByAsc(x => x.Id)
            .ForUpdate()
            .Track()
            .Build();

        var organizations = await organizationRepository.ListAsync(criteria, cancellationToken);
        if (organizations.Count != 2)
            throw new InvalidOperationException("Sender or receiver organization does not exist");
    }

    private async Task<List<OrganizationBalance>> GetBalancesAsync(
        Guid senderId,
        Guid receiverId,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<OrganizationBalance>.New()
            .Where(x => x.OrganizationId == senderId || x.OrganizationId == receiverId)
            .Track()
            .Build();

        return await userBalanceRepository.ListAsync(criteria, cancellationToken);
    }

    private async Task<OrganizationBalance> GetOrCreateBalanceAsync(
        List<OrganizationBalance> balances,
        Guid organizationId,
        int currencyId,
        CancellationToken cancellationToken)
    {
        var dbValue = balances.FirstOrDefault(
            x => x.OrganizationId == organizationId && x.CurrencyId == currencyId);
        if (dbValue is not null) return dbValue;

        dbValue = OrganizationBalance.Create(organizationId, currencyId);
        await unitOfWork.AddAsync(dbValue, cancellationToken);
        balances.Add(dbValue);

        return dbValue;
    }

    private async Task<Dictionary<Guid, OrganizationFinancialProfile>> GetFinancialProfiles(
        Guid senderId,
        Guid receiverId,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<OrganizationFinancialProfile>.New()
            .Where(x => x.OrganizationId == senderId || x.OrganizationId == receiverId)
            .OrderByAsc(x => x.OrganizationId)
            .ForUpdate()
            .Track()
            .Build();

        var profiles = (await organizationFinancialProfileRepository.ListAsync(
                criteria,
                cancellationToken))
            .ToDictionary(x => x.OrganizationId);

        foreach (var organizationId in new[] { senderId, receiverId }.Order())
        {
            if (profiles.ContainsKey(organizationId)) continue;

            var profile = systemOptions.Value.SystemId == organizationId
                ? OrganizationFinancialProfile.Create(organizationId, decimal.MinValue)
                : OrganizationFinancialProfile.Create(organizationId);
            await unitOfWork.AddAsync(profile, cancellationToken);
            profiles.Add(organizationId, profile);
        }

        return profiles;
    }

    private async Task<decimal> SumBalanceInBaseCurrencyAsync(
        IEnumerable<OrganizationBalance> balances,
        CancellationToken cancellationToken)
    {
        var result = 0m;
        foreach (var balance in balances)
        {
            result += await currencyConverter.ConvertToBaseAsync(
                balance.Balance,
                balance.CurrencyId,
                cancellationToken);
        }

        return result;
    }
}
