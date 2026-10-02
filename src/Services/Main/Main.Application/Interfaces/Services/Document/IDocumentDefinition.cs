using Main.Application.Document;
using NamedObject.Core.Interfaces;

namespace Main.Application.Interfaces.Services.Document;

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
	Type SchemaType { get; }

	Task<IDocumentResponse> GenerateAsync(
		IDocumentRequest data,
		CancellationToken token = default);
}
