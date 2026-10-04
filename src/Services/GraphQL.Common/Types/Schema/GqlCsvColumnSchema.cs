using HotChocolate;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Abstractions.Models;

namespace GraphQL.Common.Types.Schema;

[GraphQLName("CsvColumnSchema")]
public sealed record GqlCsvColumnSchema(
	[property: GraphQLIgnore] CsvColumnSchema Column)
{
	[GraphQLName("propertyName")]
	public string PropertyName => Column.PropertyName;

	[GraphQLName("names")]
	public IReadOnlyList<string> Names => Column.Names;

	[GraphQLName("type")]
	public SchemaValueType Type => Column.Type;

	[GraphQLName("required")]
	public bool Required => Column.Required;

	[GraphQLName("labelKey")]
	public string? LabelKey => Column.LabelKey;

	[GraphQLName("descriptionKey")]
	public string? DescriptionKey => Column.DescriptionKey;

	[GraphQLName("label")]
	public string? Label => Column.Label;

	[GraphQLName("description")]
	public string? Description => Column.Description;
}
