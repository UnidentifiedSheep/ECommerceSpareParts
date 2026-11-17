using System.ComponentModel.DataAnnotations;

namespace S3.Core.Options;

public record BucketOptions
{
	[Required]
	public required string Name { get; init; }

	[Required]
	public required string PublicBaseUrl { get; init; }
}
