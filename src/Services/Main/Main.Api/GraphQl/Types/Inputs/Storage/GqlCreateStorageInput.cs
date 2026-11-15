using HotChocolate;
using Main.Enums;

namespace Main.Api.GraphQl.Types.Inputs.Storage;

[GraphQLName("CreateStorageInput")]
public sealed record GqlCreateStorageInput
{
	[GraphQLName("code")]
	public required string Code { get; init; }

	[GraphQLName("description")]
	public string? Description { get; init; }

	[GraphQLName("location")]
	public string? Location { get; init; }

	[GraphQLName("type")]
	public required StorageType Type { get; init; }
}
