using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentProvider
{
	DocumentType OutputDocumentType { get; }
	DocumentType InputTemplateType { get; }

	IDocumentTemplate CreateDocument(byte[] bytes);
}
