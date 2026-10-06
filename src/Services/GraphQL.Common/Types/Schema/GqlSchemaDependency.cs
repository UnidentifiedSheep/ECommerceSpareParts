using HotChocolate;
using SchemaGeneration.Abstractions.Models;

namespace GraphQL.Common.Types.Schema;

[GraphQLName("SchemaDependency")]
public sealed record GqlSchemaDependency(
	[property: GraphQLIgnore] SchemaDependency Dependency)
{
	[GraphQLName("entityName")]
	public string EntityName => Dependency.EntityName;

	[GraphQLName("fieldName")]
	public string? FieldName => Dependency.FieldName;
}
