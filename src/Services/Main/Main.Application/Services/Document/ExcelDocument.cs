using ClosedXML.Report;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Services.Document;

public sealed class ExcelDocument(Stream stream) : IDocument
{
	public DocumentType Type => DocumentType.Excel;

	private readonly XLTemplate _template = new(stream);


	public void AddData<TData>(string key, TData data) => _template.AddVariable(key, data);

	public Task RenderAsync(Stream stream, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_template.SaveAs(stream);
		return Task.CompletedTask;
	}

	public void Dispose() => _template.Dispose();
}
