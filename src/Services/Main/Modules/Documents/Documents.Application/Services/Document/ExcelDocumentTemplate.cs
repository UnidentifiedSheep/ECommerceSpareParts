using ClosedXML.Report;
using Enums;
using Main.Application.Document.Interfaces.Document;

namespace Main.Modules.Documents.Application.Services.Document;

public sealed class ExcelDocumentTemplate : IDocumentTemplate
{

	private readonly XLTemplate _template;
	private readonly Stream _templateStream;

	internal ExcelDocumentTemplate(Stream templateStream)
	{
		_templateStream = templateStream;
		_template = new XLTemplate(templateStream);
	}
	public DocumentType Type => DocumentType.Excel;

	public void AddData<TData>(string key, TData data) => _template.AddVariable(key, data);

	public Task RenderAsync(Stream stream, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var result = _template.Generate();
		if (result.HasErrors)
			throw new InvalidOperationException(
				"Document template contains errors: " +
				string.Join("; ", result.ParsingErrors.Select(error => $"{error.Range?.RangeAddress}: {error.Message}")));

		AdjustWrappedRowHeights();
		_template.SaveAs(stream);
		return Task.CompletedTask;
	}

	private void AdjustWrappedRowHeights()
	{
		foreach (var worksheet in _template.Workbook.Worksheets)
		foreach (var row in worksheet.RowsUsed())
			if (row.CellsUsed().Any(cell => cell.Style.Alignment.WrapText))
				row.ClearHeight();
	}

	public void Dispose()
	{
		_template.Dispose();
		_templateStream.Dispose();
	}
}
