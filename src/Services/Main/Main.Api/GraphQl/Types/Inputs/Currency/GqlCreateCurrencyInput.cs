using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Currency;

[GraphQLName("CreateCurrencyInput")]
public sealed record GqlCreateCurrencyInput
{
	[GraphQLName("shortName")]
	public required string ShortName { get; init; }

	[GraphQLName("name")]
	public required string Name { get; init; }

	[GraphQLName("currencySign")]
	public required string CurrencySign { get; init; }

	[GraphQLName("code")]
	public required string Code { get; init; }
}
