using HotChocolate;
using Main.Application.Dtos.Users;

namespace Main.Api.GraphQl.Types.Inputs.User;

[GraphQLName("UserInfoInput")]
public sealed record GqlUserInfoInput
{
	[GraphQLName("name")]
	public required string Name { get; init; }

	[GraphQLName("surname")]
	public required string Surname { get; init; }

	[GraphQLName("description")]
	public string? Description { get; init; }

	public UserInfoDto ToDto() => new()
	{
		Name = Name,
		Surname = Surname,
		Description = Description
	};
}
