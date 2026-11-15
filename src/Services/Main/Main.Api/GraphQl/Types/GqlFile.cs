using HotChocolate;
using Main.Application.Dtos.Uploads;

namespace Main.Api.GraphQl.Types;

[GraphQLName("UploadedFile")]
public sealed record GqlFile(
	[property: GraphQLIgnore]
	FileDto File)
{
	[GraphQLName("key")]
	public string Key => File.Key;

	[GraphQLName("size")]
	public long Size => File.Size;

	[GraphQLName("lastModified")]
	public DateTime? LastModified => File.LastModified;

	[GraphQLName("fullPath")]
	public string FullPath => File.FullPath;
}

[GraphQLName("UploadedFilesPage")]
public sealed record GqlFilesPage(
	[property: GraphQLName("files")]
	IReadOnlyList<GqlFile> Files,
	[property: GraphQLName("nextCursor")]
	string? NextCursor,
	[property: GraphQLName("hasMore")]
	bool HasMore);
