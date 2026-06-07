namespace Abstractions.Interfaces;

public interface IS3StorageService
{
    Task<string> UploadFileAsync(string bucketName, IFile file, string keyName);
    Task<string> UploadFileAsync(string bucketName, Stream stream, string keyName, string contentType);
    Task<Stream> DownloadFileAsync(string bucketName, string keyName);
    Task<bool> DeleteFileAsync(string bucketName, string keyName);
    Task<List<string>> ListFilesAsync(
        string bucketName,
        string? lastKey,
        int size);
    Task<string> CreatePresignedUploadUrl(
        string bucketName,
        string objectKey,
        string contentType,
        TimeSpan lifetime);

    Task CompletePresignedUploadUrl(
        string bucketName,
        string objectKey,
        CancellationToken ct = default);
}