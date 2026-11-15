using HotChocolate;
using Main.Application.Dtos.Producer;

namespace Main.Api.GraphQl.Types.Inputs.Producer;

[GraphQLName("CreateProducerInput")]
public sealed record GqlCreateProducerInput
{
	[GraphQLName("name")]
	public required string Name { get; init; }

	[GraphQLName("description")]
	public string? Description { get; init; }

	public NewProducerDto ToDto()
		=> new()
		{
			Name = Name,
			Description = Description
		};
}
