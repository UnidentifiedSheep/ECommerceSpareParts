using System.ComponentModel.DataAnnotations;

namespace S3.Core.Options;

public record S3BucketsOptions
{
	public const string SectionName = "S3:Buckets";

	[Required]
	public required BucketOptions Images { get; init; }

	[Required]
	public required BucketOptions Uploads { get; init; }

	[Required]
	public required BucketOptions Documents { get; init; }
}
