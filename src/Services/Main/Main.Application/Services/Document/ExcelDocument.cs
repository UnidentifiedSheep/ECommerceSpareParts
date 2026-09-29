using ClosedXML.Report;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Services.Document;

public sealed class ExcelDocument : IDocument
{
	public DocumentType Type => DocumentType.Excel;

	private readonly XLTemplate _template;
	private readonly Stream _templateStream;

	internal ExcelDocument(Stream templateStream)
	{
		_templateStream = templateStream;
		_template = new XLTemplate(templateStream);
	}

	public void AddData<TData>(string key, TData data) => _template.AddVariable(key, data);

	public Task RenderAsync(Stream stream, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		_template.Generate();
		_template.SaveAs(stream);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		_template.Dispose();
		_templateStream.Dispose();
	}
}
