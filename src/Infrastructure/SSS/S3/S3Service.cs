using System.Net;
using Amazon.Runtime;
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
		try
		{
			var response = await internalClient.GetObjectAsync(
				new GetObjectRequest
				{
					BucketName = bucketName,
					Key = keyName
				},
				ct);
			return Response<IStreamResponse>.Success(new S3StreamResponse(response));
		}
		catch (AmazonS3Exception ex) when (ex is NoSuchKeyException || ex.ErrorCode == "NoSuchKey")
		{
			return Response<IStreamResponse>.Failure(HttpStatusCode.NotFound, "NoSuchKey");
		}
	}

	public async Task<DeleteObjectResult> DeleteFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default)
		=> (await TryDeleteFilesAsync(bucketName, [keyName], ct))[0];

	public async Task<IReadOnlyList<DeleteObjectResult>> TryDeleteFilesAsync(
		string bucketName,
		IEnumerable<string> keys,
		CancellationToken ct)
	{
		ArgumentNullException.ThrowIfNull(keys);
		var results = new List<DeleteObjectResult>();

		foreach (var batch in keys.Chunk(1000))
		{
			var request = new DeleteObjectsRequest
			{
				BucketName = bucketName,
				Objects = batch.Select(key => new KeyVersion { Key = key }).ToList(),
				Quiet = true
			};

			try
			{
				var response = await internalClient.DeleteObjectsAsync(request, ct);
				AddBatchResults(batch, response.DeleteErrors, results);
			}
			catch (DeleteObjectsException ex)
			{
				if (ex.Response?.DeleteErrors is { Count: > 0 } errors)
					AddBatchResults(batch, errors, results);
				else
					AddBatchFailure(batch, ex.ErrorCode, ex.Message, results);
			}
			catch (AmazonServiceException ex)
			{
				AddBatchFailure(batch, ex.ErrorCode, ex.Message, results);
			}
		}

		return results;
	}

	private static void AddBatchResults(
		string[] batch,
		IEnumerable<DeleteError>? errors,
		List<DeleteObjectResult> results)
	{
		var errorsByKey = new Dictionary<string, DeleteError>(StringComparer.Ordinal);
		foreach (var error in errors ?? [])
		{
			if (error.Key is null)
			{
				AddBatchFailure(batch, error.Code, error.Message, results);
				return;
			}

			errorsByKey.TryAdd(error.Key, error);
		}

		foreach (var key in batch)
			results.Add(errorsByKey.TryGetValue(key, out var error)
				? DeleteObjectResult.Fail(key, error.Code, error.Message)
				: DeleteObjectResult.Success(key));
	}

	private static void AddBatchFailure(
		IEnumerable<string> batch,
		string? errorCode,
		string errorMessage,
		List<DeleteObjectResult> results)
	{
		foreach (var key in batch)
			results.Add(DeleteObjectResult.Fail(key, errorCode, errorMessage));
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
		=> CreatePresignedUrl(bucketName, objectKey, HttpVerb.PUT, lifetime, contentType);

	public Task<string> CreatePresignedDownloadUrl(
		string bucketName,
		string objectKey,
		TimeSpan lifetime)
		=> CreatePresignedUrl(bucketName, objectKey, HttpVerb.GET, lifetime);

	private Task<string> CreatePresignedUrl(
		string bucketName,
		string objectKey,
		HttpVerb verb,
		TimeSpan lifetime,
		string? contentType = null)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime,TimeSpan.Zero);

		var request = new GetPreSignedUrlRequest
		{
			BucketName = bucketName,
			Key = objectKey,
			Verb = verb,
			Expires = DateTime.UtcNow.Add(lifetime)
		};

		if (contentType is not null) request.ContentType = contentType;

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
