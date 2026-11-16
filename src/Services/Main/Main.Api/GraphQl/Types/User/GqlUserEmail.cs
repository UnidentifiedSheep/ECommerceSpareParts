using HotChocolate;
using Main.Entities.Enums;
using Main.Modules.Identity.Application.Dtos.Users;

namespace Main.Api.GraphQl.Types.User;

[GraphQLName("UserEmail")]
public sealed record GqlUserEmail(
	[property: GraphQLIgnore]
	UserEmailDto EmailDto)
{
	[GraphQLName("email")]
	public string Email => EmailDto.Email;

	[GraphQLName("confirmed")]
	public bool Confirmed => EmailDto.Confirmed;

	[GraphQLName("emailType")]
	public EmailType EmailType => EmailDto.EmailType;

	[GraphQLName("isPrimary")]
	public bool IsPrimary => EmailDto.IsPrimary;

	[GraphQLName("confirmedAt")]
	public DateTime? ConfirmedAt => EmailDto.ConfirmedAt;

	[GraphQLName("createdAt")]
	public DateTime CreatedAt => EmailDto.CreatedAt;

	[GraphQLName("updatedAt")]
	public DateTime UpdatedAt => EmailDto.UpdatedAt;
}
