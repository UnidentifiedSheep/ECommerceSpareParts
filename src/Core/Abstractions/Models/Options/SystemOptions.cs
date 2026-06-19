using System.ComponentModel.DataAnnotations;

namespace Abstractions.Models.Options;

public sealed class SystemOptions
{
    public const string SectionName = "System";
    
    [Required]
    public required Guid SystemId { get; init; }
}