namespace Main.Application.Document.Interfaces.Document;

public interface IDocumentResponse
{
	string BucketName { get; }
	string StorageKey { get; }
}
