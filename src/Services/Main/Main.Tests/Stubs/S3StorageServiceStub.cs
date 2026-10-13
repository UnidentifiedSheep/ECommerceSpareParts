using System.Collections.Concurrent;
using System.Text;
using System.Security.Cryptography;
using S3.Core.Interfaces;
using S3.Core.Models;

namespace Tests.Stubs;

public sealed class S3StorageServiceStub : IS3Service
{
	private readonly ConcurrentDictionary<(string Bucket, string Key), byte[]> _files = new();
	private readonly ConcurrentDictionary<(string Bucket, string Key), byte> _deleteFailures = new();
	private readonly ConcurrentDictionary<(string Bucket, string Key), ConcurrentQueue<long>> _downloadOffsets = new();

	public Task<Response<IStreamResponse>> DownloadFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default)
		=> DownloadFileAsync(bucketName, keyName, 0, null, ct);

	public Task<Response<IStreamResponse>> DownloadFileAsync(
		string bucketName,
		string keyName,
		long offset,
		string? expectedETag,
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();
		ArgumentOutOfRangeException.ThrowIfNegative(offset);

		if (!_files.TryGetValue((bucketName, keyName), out var content))
			return Task.FromResult(Response<IStreamResponse>.Failure(System.Net.HttpStatusCode.NotFound, "NoSuchKey"));

		var eTag = Convert.ToHexString(SHA256.HashData(content));
		if (expectedETag is not null && expectedETag != eTag)
			throw new InvalidOperationException("S3 ETag does not match.");
		if (offset > content.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));
		_downloadOffsets.GetOrAdd((bucketName, keyName), _ => new ConcurrentQueue<long>()).Enqueue(offset);
		return Task.FromResult(Response<IStreamResponse>.Success(
			new MemoryStreamResponse(new MemoryStream(content, (int)offset, content.Length - (int)offset, false), eTag)));
	}

	public Task<string> UploadFileAsync(
		string bucketName,
		Stream stream,
		string keyName,
		string contentType) => throw new NotSupportedException();

	public async Task<DeleteObjectResult> DeleteFileAsync(
		string bucketName,
		string keyName,
		CancellationToken ct = default)
		=> (await TryDeleteFilesAsync(bucketName, [keyName], ct))[0];

	public Task<IReadOnlyList<DeleteObjectResult>> TryDeleteFilesAsync(
		string bucketName,
		IEnumerable<string> keys,
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();
		IReadOnlyList<DeleteObjectResult> results = keys.Select(key =>
		{
			if (_deleteFailures.ContainsKey((bucketName, key)))
				return DeleteObjectResult.Fail(key, "AccessDenied", "Deletion denied by test stub.");

			_files.TryRemove((bucketName, key), out _);
			return DeleteObjectResult.Success(key);
		}).ToArray();
		return Task.FromResult(results);
	}

	public Task<S3ObjectListDto> ListFilesAsync(
		string bucketName,
		string? continuationToken,
		int size,
		CancellationToken ct = default) => throw new NotSupportedException();

	public Task<string> CreatePresignedUploadUrl(
		string bucketName,
		string objectKey,
		string contentType,
		TimeSpan lifetime) => throw new NotSupportedException();

	public Task<string> CreatePresignedDownloadUrl(
		string bucketName,
		string objectKey,
		TimeSpan lifetime) => throw new NotSupportedException();

	public Task CompletePresignedUploadUrl(
		string bucketName,
		string objectKey,
		CancellationToken ct = default) => throw new NotSupportedException();

	public void SetFile(
		string bucketName,
		string key,
		string content) => _files[(bucketName, key)] = Encoding.UTF8.GetBytes(content);

	public void SetFileBytes(string bucketName, string key, byte[] content) =>
		_files[(bucketName, key)] = content;

	public IReadOnlyList<long> GetDownloadOffsets(string bucketName, string key) =>
		_downloadOffsets.TryGetValue((bucketName, key), out var offsets) ? offsets.ToArray() : [];

	public void FailDeletion(string bucketName, string key) =>
		_deleteFailures[(bucketName, key)] = 0;

	private sealed class MemoryStreamResponse(Stream stream, string eTag) : IStreamResponse
	{
		public Stream Stream { get; } = stream;
		public string? ETag => eTag;

		public void Dispose() => Stream.Dispose();
	}
}
