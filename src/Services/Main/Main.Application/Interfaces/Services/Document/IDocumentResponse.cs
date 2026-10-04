namespace Main.Application.Interfaces.Services.Document;

public interface IDocumentResponse
{
	string GeneratedFileLink { get; }
	string BucketName { get; }
	string StorageKey { get; }
}
