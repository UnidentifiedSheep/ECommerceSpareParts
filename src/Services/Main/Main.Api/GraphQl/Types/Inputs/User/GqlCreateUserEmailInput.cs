using HotChocolate;
using Main.Application.Dtos.Emails;
using Main.Enums;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("CreateUserEmailInput")]
public sealed record GqlCreateUserEmailInput
{
	[GraphQLName("email")]
	public required string Email { get; init; }

	[GraphQLName("type")]
	public required EmailType Type { get; init; }

	[GraphQLName("isPrimary")]
	public required bool IsPrimary { get; init; }

	[GraphQLName("isConfirmed")]
	public required bool IsConfirmed { get; init; }

	public EmailDto ToDto() => new()
	{
		Email = Email,
		Type = Type,
		IsPrimary = IsPrimary,
		IsConfirmed = IsConfirmed
	};
}
