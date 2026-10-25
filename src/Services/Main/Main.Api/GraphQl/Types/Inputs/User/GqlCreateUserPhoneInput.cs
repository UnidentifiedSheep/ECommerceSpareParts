using HotChocolate;
using Main.Application.Dtos.Users;
using Main.Enums;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("CreateUserPhoneInput")]
public sealed record GqlCreateUserPhoneInput
{
	[GraphQLName("number")]
	public required string Number { get; init; }

	[GraphQLName("type")]
	public required PhoneType Type { get; init; }

	[GraphQLName("isPrimary")]
	public required bool IsPrimary { get; init; }

	[GraphQLName("isConfirmed")]
	public required bool IsConfirmed { get; init; }

	public UserPhoneDto ToDto() => new()
	{
		Number = Number,
		Type = Type,
		IsPrimary = IsPrimary,
		IsConfirmed = IsConfirmed
	};
}
