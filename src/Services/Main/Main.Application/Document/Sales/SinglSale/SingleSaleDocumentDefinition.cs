using Main.Application.Interfaces.Services.Document;

namespace Main.Application.Document.Sales.SinglSale;

public class SingleSaleDocumentDefinition(
	) : IDocumentDefinition<SingleSaleDocumentRequest>
{
	public string SystemName => "SingleSale";
	public Task GenerateAsync(
		SingleSaleDocumentRequest request,
		Stream destination,
		CancellationToken token = default)
	{

	}
}
