using Microsoft.AspNetCore.Http;

namespace Integrations.Interfaces;

public interface IS3StorageService
{
    Task<string> UploadFileAsync(IFormFile file, string keyName);
    Task<string> UploadFileAsync(Stream stream, string keyName, string contentType);
    Task<Stream> DownloadFileAsync(string keyName);
    Task<bool> DeleteFileAsync(string keyName);
    Task<List<string>> ListFilesAsync();
}