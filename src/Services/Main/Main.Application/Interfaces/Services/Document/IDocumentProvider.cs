using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentProvider
{
	DocumentType SupportedType { get; }

	IDocumentTemplate CreateDocument(byte[] bytes);
}
