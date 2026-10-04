namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentResponse
{
	string BucketName { get; }
	string StorageKey { get; }
}
