using HotChocolate;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Abstractions.Models;

namespace GraphQL.Common.Types.Schema;

[GraphQLName("FieldSchema")]
public sealed record GqlFieldSchema(
	[property: GraphQLIgnore] FieldSchema Field)
{
	[GraphQLName("name")]
	public string Name => Field.Name;

	[GraphQLName("type")]
	public SchemaValueType Type => Field.Type;

	[GraphQLName("labelKey")]
	public string? LabelKey => Field.LabelKey;

	[GraphQLName("descriptionKey")]
	public string? DescriptionKey => Field.DescriptionKey;

	[GraphQLName("label")]
	public string? Label => Field.Label;

	[GraphQLName("description")]
	public string? Description => Field.Description;

	[GraphQLName("required")]
	public bool Required => Field.Required;

	[GraphQLName("control")]
	public InputControlType? Control => Field.Control;

	[GraphQLName("accepts")]
	public IReadOnlyList<string> Accepts => Field.Accepts;

	[GraphQLName("dependency")]
	public GqlSchemaDependency? Dependency => Field.Dependency is { } dependency
		? new GqlSchemaDependency(dependency)
		: null;

	[GraphQLName("nestedSchema")]
	public GqlObjectSchema? NestedSchema => Field.NestedSchema is { } nestedSchema
		? new GqlObjectSchema(nestedSchema)
		: null;
}
