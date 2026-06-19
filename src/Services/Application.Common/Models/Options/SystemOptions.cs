namespace Application.Common.Models.Options;

public sealed class SystemOptions
{
    public const string SectionName = "System";
    
    public required Guid SystemId { get; init; }
}