using HotChocolate;

namespace Main.Api.GraphQl.Types.Document;

[GraphQLName("DocumentLink")]
public sealed record GqlDocumentLink(
	[property: GraphQLName("url")] string Url,
	[property: GraphQLName("expiresAtUtc")] DateTime ExpiresAtUtc);
