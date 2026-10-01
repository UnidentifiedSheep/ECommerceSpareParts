using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document.Sales.SingleSale;

public class SingleSaleDocumentDefinition(
	IDocumentTemplateResolver templateResolver
	) : DocumentDefinitionBase<SingleSaleDocumentRequest>(
		templateResolver,
		[
			(DocumentType.Excel, DocumentType.Excel)
		])
{
	public override string SystemName => "SingleSale";
	protected override string BaseTemplateKey => "Sales/SingleSale/Current";

	public override Task GenerateAsync(
		SingleSaleDocumentRequest request,
		Stream destination,
		CancellationToken token = default)
		=> throw new NotImplementedException();
}
