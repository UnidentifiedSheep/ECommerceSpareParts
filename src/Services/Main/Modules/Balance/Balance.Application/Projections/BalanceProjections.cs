using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Balances;
using Main.Entities.Balance;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class TransactionDtoProjectionProvider : ProjectionProviderBase<Transaction, TransactionDto>
{
	public override Expression<Func<Transaction, TransactionDto>> Projection { get; } =
		x => new TransactionDto
		{
			Amount = x.Amount,
			Id = x.Id,
			CurrencyId = x.CurrencyId,
			ReceiverId = x.ReceiverId,
			SenderId = x.SenderId,
			Status = x.Status,
			Type = x.Type,
			TransactionDate = x.TransactionDatetime,
			SourceType = x.SourceType,
			ReversedAt = x.ReversedAt,
			ReversedBy = x.ReversedBy
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	OrganizationBalanceDtoProjectionProvider : ProjectionProviderBase<OrganizationBalance,
	OrganizationBalanceDto>
{
	public override Expression<Func<OrganizationBalance, OrganizationBalanceDto>> Projection { get; } =
		x => new OrganizationBalanceDto
		{
			Balance = x.Balance, CurrencyId = x.CurrencyId
		};
}

public static class OrganizationFinancialProfileDtoFactory
{
	public static OrganizationFinancialProfileDto Create(
		OrganizationFinancialProfile profile,
		decimal netPositionInBaseCurrency)
		=> new()
		{
			NetPositionInBaseCurrency = netPositionInBaseCurrency,
			MinimalAllowedBalance = profile.MinAllowedBalance
		};
}
