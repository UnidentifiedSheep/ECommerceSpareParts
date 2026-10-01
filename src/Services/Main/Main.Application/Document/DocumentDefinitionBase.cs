using Main.Application.Interfaces.Services.Document;

namespace Main.Application.Document;

public abstract class DocumentDefinitionBase<TRequest> :
	IDocumentDefinition<TRequest> where TRequest : IDocumentRequest
{
	public abstract string SystemName { get; }

	public abstract Task GenerateAsync(
		TRequest request,
		Stream destination,
		CancellationToken token = default);


}
