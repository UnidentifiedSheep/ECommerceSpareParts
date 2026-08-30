using HotChocolate;
using Main.Application.Dtos.Currencies;

namespace Main.Api.GraphQl.Types;

[GraphQLName("Currency")]
public sealed record GqlCurrency(
    [property: GraphQLIgnore]
    CurrencyDto Currency)
{
    [GraphQLName("id")]
    public int Id => Currency.Id;

    [GraphQLName("shortName")]
    public string ShortName => Currency.ShortName;

    [GraphQLName("name")]
    public string Name => Currency.Name;

    [GraphQLName("sign")]
    public string Sign => Currency.CurrencySign;

    [GraphQLName("code")]
    public string Code => Currency.Code;
}
