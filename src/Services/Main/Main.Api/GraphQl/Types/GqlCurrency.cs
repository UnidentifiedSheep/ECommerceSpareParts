using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Types;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Currencies.GetCurrencyHistory;
using MediatR;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Currency")]
public sealed record GqlCurrency(
	[property: GraphQLIgnore]
	CurrencyDto Currency)
{
	[GraphQLName("id")]
	[Shareable]
	public int Id => Currency.Id;

	[GraphQLName("shortName")]
	public string ShortName => Currency.ShortName;

	[GraphQLName("name")]
	public string Name => Currency.Name;

	[GraphQLName("sign")]
	public string Sign => Currency.CurrencySign;

	[GraphQLName("code")]
	public string Code => Currency.Code;

	[GraphQLName("history")]
	[RequireAllPermissions(PermissionCodes.CURRENCIES_GET)]
	public async Task<IReadOnlyList<GqlCurrencyRateHistory>> GetHistoryAsync(
		GqlPagination pagination,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetCurrencyHistoryQuery(Id, pagination), cancellationToken);
		return result.History.Select(history => new GqlCurrencyRateHistory(history)).ToArray();
	}
}
