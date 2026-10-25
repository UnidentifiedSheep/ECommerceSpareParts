using HotChocolate;
using Main.Application.Dtos.Users;

namespace Main.Api.GraphQl.Types.User;

[GraphQLName("UserInfo")]
public record GqlUserInfo(
	[property: GraphQLIgnore] UserInfoDto Info)
{
	[GraphQLName("name")]
	public string Name => Info.Name;

	[GraphQLName("surname")]
	public string Surname => Info.Surname;

	[GraphQLName("description")]
	public string? Description => Info.Description;
}
