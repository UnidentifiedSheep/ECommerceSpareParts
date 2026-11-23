using Enums;
using Locan.Core.Interfaces;
using NamedObject.Core.Interfaces;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentDefinition<in TRequest, TResponse> : IDocumentDefinition
	where TRequest : IDocumentRequest
	where TResponse : IDocumentResponse
{
	Task<TResponse> GenerateAsync(
		TRequest request,
		CancellationToken token = default);
}

public interface IDocumentDefinition : INamedObject
{
	string DocumentGroup { get; }
	IReadOnlyCollection<PermissionCodes> RequiredPermissions { get; }
	Type SchemaType { get; }
	Type RequestType { get; }

	ILocalizableMessage Name { get; }
	ILocalizableMessage Description { get; }
	bool SupportsDocumentType(DocumentType documentType);

	Task<IDocumentResponse> GenerateAsync(
		IDocumentRequest data,
		CancellationToken token = default);

	Task RenderAsync(
		object schemaData,
		DocumentType documentType,
		string culture,
		Stream output,
		CancellationToken token = default);
}
