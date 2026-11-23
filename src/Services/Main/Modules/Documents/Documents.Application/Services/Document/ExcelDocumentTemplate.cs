using ClosedXML.Excel;
using ClosedXML.Report;
using Enums;
using Main.Application.Document.Interfaces.Document;

namespace Main.Modules.Documents.Application.Services.Document;

public sealed class ExcelDocumentTemplate : IDocumentTemplate
{
	public const string HiddenRowMarker = "#HIDE_ROW#";

	private const double DefaultFontSize = 11;
	private const double LineHeightFactor = 1.35;
	private const double RowPadding = 3;

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

		HideMarkedRows();
		AdjustWrappedRowHeights();
		AdjustMergedWrappedRowHeights();
		_template.SaveAs(stream);
		return Task.CompletedTask;
	}

	private void HideMarkedRows()
	{
		foreach (var worksheet in _template.Workbook.Worksheets)
		{
			var markedCells = worksheet
				.CellsUsed(cell => cell.DataType == XLDataType.Text && cell.GetString() == HiddenRowMarker)
				.ToList();
			foreach (var cell in markedCells)
			{
				cell.Clear(XLClearOptions.Contents);
				cell.WorksheetRow().Hide();
			}
		}
	}

	private void AdjustWrappedRowHeights()
	{
		foreach (var worksheet in _template.Workbook.Worksheets)
		foreach (var row in worksheet.RowsUsed())
			if (!row.IsHidden && row.CellsUsed().Any(cell => cell.Style.Alignment.WrapText))
				row.ClearHeight();
	}

	private void AdjustMergedWrappedRowHeights()
	{
		foreach (var worksheet in _template.Workbook.Worksheets)
		foreach (var range in worksheet.MergedRanges.Where(range => range.RowCount() == 1))
		{
			var row = range.FirstRow().WorksheetRow();
			var cell = range.FirstCell();
			if (row.IsHidden || !cell.Style.Alignment.WrapText)
				continue;

			var text = cell.GetFormattedString();
			if (text.Length == 0)
				continue;

			var fontSize = cell.Style.Font.FontSize;
			var width = range.Columns().Sum(column => column.WorksheetColumn().Width);
			var charsPerLine = Math.Max(1, width * DefaultFontSize / fontSize);
			var lines = text
				.Split('\n')
				.Sum(line => Math.Max(1, (int)Math.Ceiling(line.Length / charsPerLine)));
			if (lines <= 1)
				continue;

			row.Height = Math.Max(row.Height, lines * fontSize * LineHeightFactor + RowPadding);
		}
	}

	public void Dispose()
	{
		_template.Dispose();
		_templateStream.Dispose();
	}
}
