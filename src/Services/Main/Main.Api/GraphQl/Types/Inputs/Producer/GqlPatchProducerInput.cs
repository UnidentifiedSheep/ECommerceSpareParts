using GraphQL.Common.Types;
using HotChocolate;
using Main.Modules.Product.Application.Dtos.Producer;

namespace Main.Api.GraphQl.Types.Inputs.Producer;

[GraphQLName("PatchProducerInput")]
public sealed record GqlPatchProducerInput
{
	[GraphQLName("name")]
	public GqlPatchField<string>? Name { get; init; }

	[GraphQLName("description")]
	public GqlPatchField<string?>? Description { get; init; }

	public PatchProducerDto ToDto()
		=> new()
		{
			Name = Name,
			Description = Description
		};
}
