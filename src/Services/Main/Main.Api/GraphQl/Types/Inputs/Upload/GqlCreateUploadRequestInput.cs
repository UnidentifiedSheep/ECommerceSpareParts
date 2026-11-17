using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Upload;

[GraphQLName("CreateUploadRequestInput")]
public sealed record GqlCreateUploadRequestInput
{
	[GraphQLName("fileName")]
	public required string FileName { get; init; }

	[GraphQLName("contentType")]
	public required string ContentType { get; init; }
}
