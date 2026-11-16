using GraphQL.Common.Types;
using HotChocolate;
using Main.Modules.Organization.Application.Dtos.Organizations;

namespace Main.Api.GraphQl.Types.Inputs.Organization;

[GraphQLName("UpdateOrganizationInput")]
public sealed record GqlUpdateOrganizationInput
{
	[GraphQLName("organizationId")]
	public required Guid OrganizationId { get; init; }

	[GraphQLName("name")]
	public GqlPatchField<string>? Name { get; init; }

	[GraphQLName("isHidden")]
	public GqlPatchField<bool>? IsHidden { get; init; }

	public PatchOrganizationDto ToDto()
		=> new()
		{
			Name = Name,
			IsHidden = IsHidden
		};
}
