using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document.Sales.SingleSale;

public class SingleSaleDocumentDefinition(
	IDocumentTemplateResolver templateResolver
	) : DocumentDefinitionBase<SingleSaleDocumentRequest>(
		templateResolver)
{
	private static readonly DocumentTypePair[] SupportedTypes = [new(DocumentType.Excel, DocumentType.Excel)];

	public override string SystemName => "SingleSale";
	protected override string BaseTemplateKey => "Sales/SingleSale/Current";
	protected override DocumentTypePair[] SupportedTypePairs => SupportedTypes;

	public override async Task GenerateAsync(
		SingleSaleDocumentRequest request,
		Stream destination,
		CancellationToken token = default)
	{
		var template = await GetTemplateAsync(request, token);
	}
}
