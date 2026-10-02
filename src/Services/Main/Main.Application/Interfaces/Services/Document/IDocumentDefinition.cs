using Main.Application.Document;
using NamedObject.Core.Interfaces;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentDefinition<in TRequest, TResponse> : IDocumentDefinition
	where TRequest : IDocumentRequest
    where TResponse : IDocumentResponse
{
	Task<TResponse> GenerateAsync(
		TRequest request,
		Stream destination,
		CancellationToken token = default);
}

public interface IDocumentDefinition : INamedObject
{
	Type SchemaType { get; }

	Task<IDocumentResponse> GenerateAsync(
		IDocumentRequest data,
		Stream destination,
		CancellationToken token = default);
}
