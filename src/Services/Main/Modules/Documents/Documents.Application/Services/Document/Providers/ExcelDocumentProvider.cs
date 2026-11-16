using Enums;
using Main.Application.Document.Interfaces.Document;

namespace Main.Modules.Documents.Application.Services.Document.Providers;

public sealed class ExcelDocumentProvider : IDocumentProvider
{
	public DocumentType OutputDocumentType => DocumentType.Excel;
	public DocumentType InputTemplateType => DocumentType.Excel;

	public IDocumentTemplate CreateDocument(byte[] bytes)
	{
		var stream = new MemoryStream(bytes, false);
		try
		{
			return new ExcelDocumentTemplate(stream);
		}
		catch
		{
			stream.Dispose();
			throw;
		}
	}
}
