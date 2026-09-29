using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Enums;
using Microsoft.Extensions.DependencyInjection;
using S3.Core.Interfaces;
using S3.Core.Models;

namespace S3;

public sealed class S3Service(
	[FromKeyedServices(Visibility.Internal)]
	IAmazonS3 internalClient,
	[FromKeyedServices(Visibility.External)]
	IAmazonS3 externalClient) : IS3Service
{
	public async Task<string> UploadFileAsync(
		string bucketName,
		Stream stream,
		string keyName,
		string contentType)
	{
		var request = new PutObjectRequest
		{
			BucketName = bucketName,
			Key = keyName,
			InputStream = stream,
			ContentType = contentType,
			UseChunkEncoding = false
		};

		await internalClient.PutObjectAsync(request);
		return keyName;
	}

	public async Task<Response<IStreamResponse>> DownloadFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default)
	{
		var request = new GetObjectRequest
		{
			BucketName = bucketName, Key = keyName
		};

		try
		{
			var response = await internalClient.GetObjectAsync(request, ct);
			return Response<IStreamResponse>.Success(new S3StreamResponse(response));
		}
		catch (AmazonS3Exception ex) when (ex is NoSuchKeyException || ex.ErrorCode == "NoSuchKey")
		{
			return Response<IStreamResponse>.Failure(HttpStatusCode.NotFound, "NoSuchKey");
		}
	}

	public async Task<bool> DeleteFileAsync(string bucketName, string keyName)
	{
		var request = new DeleteObjectRequest
		{
			BucketName = bucketName, Key = keyName
		};

		var response = await internalClient.DeleteObjectAsync(request);
		return response.HttpStatusCode == HttpStatusCode.NoContent;
	}

	public async Task<S3ObjectListDto> ListFilesAsync(
		string bucketName,
		string? continuationToken,
		int size,
		CancellationToken ct = default)
	{
		var request = new ListObjectsV2Request
		{
			BucketName = bucketName,
			ContinuationToken = string.IsNullOrWhiteSpace(continuationToken) ? null : continuationToken,
			MaxKeys = size
		};

		var response = await internalClient.ListObjectsV2Async(request, ct);
		var files = (response.S3Objects ?? [])
			.Select(o => new S3ObjectDto
			{
				Key = o.Key,
				LastModified = o.LastModified,
				Size = o.Size ?? 0
			})
			.ToList();

		return new S3ObjectListDto
		{
			Files = files,
			NextContinuationToken = response.NextContinuationToken,
			HasMore = response.IsTruncated ?? false
		};
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

		return externalClient.GetPreSignedURLAsync(request);
	}

	public Task CompletePresignedUploadUrl(
		string bucketName,
		string objectKey,
		CancellationToken ct = default)
	{
		var request = new GetObjectMetadataRequest
		{
			BucketName = bucketName, Key = objectKey
		};

		return internalClient.GetObjectMetadataAsync(request, ct);
	}
}
