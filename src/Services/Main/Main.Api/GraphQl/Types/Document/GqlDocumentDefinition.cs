using GraphQL.Common.Types.Schema;
using HotChocolate;
using Main.Application.Dtos.Documents;
using Main.Enums.Documents;

namespace Main.Api.GraphQl.Types.Document;

[GraphQLName("DocumentDefinition")]
public sealed record GqlDocumentDefinition(
	[property: GraphQLIgnore] DocumentDefinitionDto Definition)
{
	[GraphQLName("systemName")]
	public string SystemName => Definition.SystemName;

	[GraphQLName("documentGroup")]
	public string DocumentGroup => Definition.DocumentGroup;

	[GraphQLName("name")]
	public string Name => Definition.Name;

	[GraphQLName("description")]
	public string Description => Definition.Description;

	[GraphQLName("supportedDocumentTypes")]
	public IReadOnlyList<DocumentType> SupportedDocumentTypes => Definition.SupportedDocumentTypes;

	[GraphQLName("requestSchema")]
	public GqlObjectSchema RequestSchema => new(Definition.RequestSchema);

	[GraphQLName("templateSchema")]
	public GqlObjectSchema TemplateSchema => new(Definition.TemplateSchema);
}
