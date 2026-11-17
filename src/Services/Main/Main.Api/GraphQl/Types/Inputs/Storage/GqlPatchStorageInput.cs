using GraphQL.Common.Types;
using HotChocolate;
using Main.Entities.Storage.Enums;
using Main.Modules.Storage.Application.Dtos.Storage;

namespace Main.Api.GraphQl.Types.Inputs.Storage;

[GraphQLName("PatchStorageInput")]
public sealed record GqlPatchStorageInput
{
	[GraphQLName("description")]
	public GqlPatchField<string?>? Description { get; init; }

	[GraphQLName("location")]
	public GqlPatchField<string?>? Location { get; init; }

	[GraphQLName("type")]
	public GqlPatchField<StorageType>? Type { get; init; }

	public PatchStorageDto ToDto()
		=> new()
		{
			Description = Description,
			Location = Location,
			Type = Type
		};
}
