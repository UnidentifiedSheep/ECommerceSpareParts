using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Upload;

[GraphQLName("GetUploadsInput")]
public sealed record GqlGetUploadsInput
{
	[GraphQLName("cursor")]
	public string? Cursor { get; init; }

	[GraphQLName("size")]
	public required int Size { get; init; }
}
