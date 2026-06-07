using System.Net;
using Abstractions.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3;

public class S3StorageService(IAmazonS3 s3Client) : IS3StorageService
{
    public async Task<string> UploadFileAsync(string bucketName, IFile file, string keyName)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = keyName,
            InputStream = memoryStream,
            ContentType = file.ContentType,
            UseChunkEncoding = false
        };

        await s3Client.PutObjectAsync(request);
        return keyName;
    }

    public async Task<string> UploadFileAsync(string bucketName, Stream stream, string keyName, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = keyName,
            InputStream = stream,
            ContentType = contentType,
            UseChunkEncoding = false
        };

        await s3Client.PutObjectAsync(request);
        return keyName;
    }

    public async Task<Stream> DownloadFileAsync(string bucketName, string keyName)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = keyName
        };

        using var response = await s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string keyName)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = keyName
        };

        var response = await s3Client.DeleteObjectAsync(request);
        return response.HttpStatusCode == HttpStatusCode.NoContent;
    }

    public async Task<List<string>> ListFilesAsync(
        string bucketName,
        string? lastKey,
        int size)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = bucketName,
            StartAfter = lastKey,
            MaxKeys = size
        };

        var response = await s3Client.ListObjectsV2Async(request);
        return response.S3Objects.Select(o => o.Key).ToList();
    }
    
    public Task<string> CreatePresignedUploadUrl(
        string bucketName,
        string objectKey,
        string contentType,
        TimeSpan lifetime)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(lifetime),
            ContentType = contentType
        };

        return s3Client.GetPreSignedURLAsync(request);
    }

    public Task CompletePresignedUploadUrl(
        string bucketName,
        string objectKey,
        CancellationToken ct = default)
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };
        return s3Client.CompleteMultipartUploadAsync(request, ct);
    }
}