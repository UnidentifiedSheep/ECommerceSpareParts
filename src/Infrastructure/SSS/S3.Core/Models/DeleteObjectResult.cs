namespace S3.Core.Models;

public sealed record DeleteObjectResult(
	string Key,
	bool IsSuccess,
	string? ErrorCode = null,
	string? ErrorMessage = null)
{
	public static DeleteObjectResult Success(string key) => new(key, true);

	public static DeleteObjectResult Fail(
		string key,
		string? errorCode,
		string? errorMessage) => new(key, false, errorCode, errorMessage);
}
