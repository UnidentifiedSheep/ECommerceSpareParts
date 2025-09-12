using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Integrations.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Integrations.S3;

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["S3Storage:BucketName"] ?? throw new ArgumentNullException(null, "BucketName is missing");
    }
    
    public async Task<string> UploadFileAsync(IFormFile file, string keyName)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = keyName,
            InputStream = memoryStream,
            ContentType = file.ContentType,
            UseChunkEncoding = false
        };

        await _s3Client.PutObjectAsync(request);
        return keyName;
    }

    public async Task<string> UploadFileAsync(Stream stream, string keyName, string contentType)
    {

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = keyName,
            InputStream = stream,
            ContentType = contentType,
            UseChunkEncoding = false
        };

        await _s3Client.PutObjectAsync(request);
        return keyName;
    }

    public async Task<Stream> DownloadFileAsync(string keyName)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = keyName,
        };

        using var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }

    public async Task<bool> DeleteFileAsync(string keyName)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = keyName
        };

        var response = await _s3Client.DeleteObjectAsync(request);
        return response.HttpStatusCode == HttpStatusCode.NoContent;
    }

    public async Task<List<string>> ListFilesAsync()
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        return response.S3Objects.Select(o => o.Key).ToList();
    }
}