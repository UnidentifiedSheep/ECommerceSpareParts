using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Options.S3;

public record BucketOptions
{
	[Required]
	public required string Name { get; init; }

	[Required]
	public required string PublicBaseUrl { get; init; }
}
