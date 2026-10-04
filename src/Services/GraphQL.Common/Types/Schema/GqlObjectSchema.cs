using HotChocolate;
using SchemaGeneration.Abstractions.Models;

namespace GraphQL.Common.Types.Schema;

[GraphQLName("ObjectSchema")]
public sealed record GqlObjectSchema(
	[property: GraphQLIgnore] ObjectSchema Schema)
{
	[GraphQLName("version")]
	public int Version => Schema.Version;

	[GraphQLName("fields")]
	public IReadOnlyList<GqlFieldSchema> Fields => Schema.Fields
		.Select(schemaField => new GqlFieldSchema(schemaField))
		.ToArray();

	[GraphQLName("csvSchema")]
	public GqlCsvSchema? CsvSchema => Schema.CsvSchema is { } csvSchema
		? new GqlCsvSchema(csvSchema)
		: null;
}
