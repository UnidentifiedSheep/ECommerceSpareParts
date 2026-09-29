using Main.Enums.Documents;

namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentRequest
{
	DocumentType DocumentType { get; }
}
