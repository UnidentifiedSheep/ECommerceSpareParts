using Enums;

namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentRequest
{
	DocumentType DocumentType { get; }
	string Culture { get; }
}
