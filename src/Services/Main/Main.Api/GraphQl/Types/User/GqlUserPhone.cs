using HotChocolate;
using Main.Application.Dtos.Users;
using Main.Enums;

namespace Main.Api.GraphQl.Types.User;

[GraphQLName("UserPhone")]
public record GqlUserPhone(
	[property: GraphQLIgnore] UserPhoneDto Dto)
{
	[GraphQLName("number")]
	public string Number => Dto.Number;

	[GraphQLName("type")]
	public PhoneType Type => Dto.Type;

	[GraphQLName("isConfirmed")]
	public bool IsConfirmed => Dto.IsConfirmed;

	[GraphQLName("isPrimary")]
	public bool IsPrimary => Dto.IsPrimary;
}
