using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Services.Document.Providers;

public sealed class ExcelDocumentProvider : IDocumentProvider
{
	public DocumentType SupportedType => DocumentType.Excel;

	public IDocumentTemplate CreateDocument(byte[] bytes)
	{
		var stream = new MemoryStream(bytes, writable: false);
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
