using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Options.S3;

public record S3BucketsOptions
{
    public const string SectionName = "S3:Buckets";
    
    [Required]
    public required BucketOptions Images { get; init; }
    
    [Required]
    public required BucketOptions Uploads { get; init; }
}