using Application.Common.Interfaces.Cqrs;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Dtos.Documents;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;
using NamedObject.Core.Interfaces;
using SchemaGeneration.Abstractions;

namespace Main.Application.Handlers.Documents;

public sealed record GetDocumentDefinitionsQuery : IQuery<GetDocumentDefinitionsResult>;

public sealed record GetDocumentDefinitionsResult(IReadOnlyList<DocumentDefinitionDto> Definitions);

public sealed class GetDocumentDefinitionsHandler(
	INamedObjectRegistry<IDocumentDefinition> registry,
	IContextualLocalizer localizer,
	ISchemaGenerator schemaGenerator)
	: IQueryHandler<GetDocumentDefinitionsQuery, GetDocumentDefinitionsResult>
{
	public Task<GetDocumentDefinitionsResult> Handle(
		GetDocumentDefinitionsQuery request,
		CancellationToken cancellationToken)
	{
		var definitions = registry.All
			.Select(definition => new DocumentDefinitionDto
			{
				SystemName = definition.SystemName,
				DocumentGroup = definition.DocumentGroup,
				Name = localizer.Get(definition.Name),
				Description = localizer.Get(definition.Description),
				SupportedDocumentTypes = Enum.GetValues<DocumentType>()
					.Where(definition.SupportsDocumentType)
					.ToArray(),
				RequestSchema = schemaGenerator.Generate(definition.RequestType),
				TemplateSchema = schemaGenerator.Generate(definition.SchemaType)
			})
			.OrderBy(definition => definition.DocumentGroup, StringComparer.OrdinalIgnoreCase)
			.ThenBy(definition => definition.SystemName, StringComparer.OrdinalIgnoreCase)
			.ToArray();

		return Task.FromResult(new GetDocumentDefinitionsResult(definitions));
	}
}
