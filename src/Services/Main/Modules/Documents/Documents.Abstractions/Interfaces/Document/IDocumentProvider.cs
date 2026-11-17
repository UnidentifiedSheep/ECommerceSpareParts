using Enums;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentProvider
{
	DocumentType OutputDocumentType { get; }
	DocumentType InputTemplateType { get; }

	IDocumentTemplate CreateDocument(byte[] bytes);
}
