using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Organizations;
using Main.Entities.Balance;
using Main.Entities.Currency;
using Main.Entities.Organization;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class TransactionDtoProjectionProvider
    : IProjectionProvider<Transaction, TransactionDto>
{
    public TransactionDtoProjectionProvider(
        IProjectionProvider<Organization, OrganizationDto> organizationProjection)
    {
        var organizationToDto = organizationProjection.Projection;

        Projection = x => new TransactionDto
        {
            Amount = x.Amount,
            Id = x.Id,
            CurrencyId = x.CurrencyId,
            Receiver = organizationToDto.Invoke(x.Receiver),
            Sender = organizationToDto.Invoke(x.Sender),
            Status = x.Status,
            Type = x.Type,
            TransactionDate = x.TransactionDatetime,
            SourceType = x.SourceType
        };
    }

    public Expression<Func<Transaction, TransactionDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class OrganizationBalanceDtoProjectionProvider
    : IProjectionProvider<OrganizationBalance, OrganizationBalanceDto>
{
    public OrganizationBalanceDtoProjectionProvider(
        IProjectionProvider<Currency, CurrencyDto> currencyProjection)
    {
        var currencyToDto = currencyProjection.Projection;

        Projection = x => new OrganizationBalanceDto
        {
            Balance = x.Balance,
            Currency = currencyToDto.Invoke(x.Currency)
        };
    }

    public Expression<Func<OrganizationBalance, OrganizationBalanceDto>> Projection { get; }
}

public static class OrganizationFinancialProfileDtoFactory
{
    public static OrganizationFinancialProfileDto Create(
        OrganizationFinancialProfile profile,
        decimal netPositionInBaseCurrency)
    {
        return new OrganizationFinancialProfileDto
        {
            NetPositionInBaseCurrency = netPositionInBaseCurrency,
            MinimalAllowedBalance = profile.MinAllowedBalance
        };
    }
}
