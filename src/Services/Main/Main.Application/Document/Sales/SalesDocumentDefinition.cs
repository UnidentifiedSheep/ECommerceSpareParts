using Main.Application.Interfaces.Services.Document;

namespace Main.Application.Document.Sales;

public sealed class SalesDocumentDefinition(
	IEnumerable<IDocumentProvider> providers
	) : IDocumentDefinition<SalesDocumentRequest>
{
	public string SystemName => "Sales";

	public Task GenerateAsync(
		SalesDocumentRequest request,
		Stream destination,
		CancellationToken token = default) => throw new NotImplementedException();
}
