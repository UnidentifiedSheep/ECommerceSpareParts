using HotChocolate;
using SchemaGeneration.Abstractions.Models;

namespace GraphQL.Common.Types.Schema;

[GraphQLName("CsvSchema")]
public sealed record GqlCsvSchema(
	[property: GraphQLIgnore] CsvSchema Schema)
{
	[GraphQLName("columns")]
	public IReadOnlyList<GqlCsvColumnSchema> Columns => Schema.Columns
		.Select(column => new GqlCsvColumnSchema(column))
		.ToArray();
}
